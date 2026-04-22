using MediatR;
using Platform.Application.Abstractions.Data;
using Platform.Application.Messaging;
using Platform.BuildingBlocks.Responses;
using Platform.Contracts.Messages.Emails;
using Platform.Messaging.Abstractions;
using Platform.Ordering.API.Application.Abstractions.Integrations.Catalog;
using Platform.Ordering.API.Application.Abstractions.Payments;
using Platform.Ordering.API.Application.Features.Orders.Shared;
using Platform.Ordering.API.Domain.Enums;
using Platform.Ordering.API.Infrastructure.Persistence.Models;

namespace Platform.Ordering.API.Application.Features.Orders.Commands.Webhook;

public sealed class ProcessWebhookHandler : ICommandHandler<ProcessWebhookCommand>
{
    private readonly ICatalogClient _catalogClient;
    private readonly IMessagePublisher _messagePublisher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPaymentService _paymentService;

    public ProcessWebhookHandler(
        ICatalogClient catalogClient,
        IMessagePublisher messagePublisher,
        IUnitOfWork unitOfWork,
        IPaymentService paymentService)
    {
        _catalogClient = catalogClient;
        _messagePublisher = messagePublisher;
        _unitOfWork = unitOfWork;
        _paymentService = paymentService;
    }

    public async Task<Result<Unit>> Handle(ProcessWebhookCommand command, CancellationToken cancellationToken)
    {
        var data = await _paymentService.VerifyWebhook(command.Request.RawBody);
        if (data == null)
            return Result<Unit>.Success(Unit.Value);

        var orderModel = await _unitOfWork.GetRepository<OrderModel>().FindAsync(
            x => x.OrderCode == data.OrderCode,
            false,
            cancellationToken,
            x => x.Items);

        if (orderModel is null)
            return Result<Unit>.Success(Unit.Value);

        var order = orderModel.ToDomain();

        if (order.Status == OrderStatus.Paid || order.Status == OrderStatus.Cancelled)
            return Result<Unit>.Success(Unit.Value);

        var paymentModel = await _unitOfWork.GetRepository<PaymentModel>().FindAsync(
            x => x.OrderId == order.Id,
            false,
            cancellationToken);

        if (paymentModel is null)
            return Result<Unit>.Success(Unit.Value);

        var payment = paymentModel.ToDomain();

        if (payment.Status == PaymentStatus.Paid)
            return Result<Unit>.Success(Unit.Value);

        if (data.Code == "00")
        {
            var markPaymentResult = payment.MarkAsPaid();
            if (markPaymentResult.IsFailure)
                return Result<Unit>.Success(Unit.Value);

            var markOrderResult = order.MarkAsPaid();
            if (markOrderResult.IsFailure)
                return Result<Unit>.Success(Unit.Value);
        }
        else
        {
            var stockItems = order.Items.Select(item => new StockAdjustmentItem
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity
            }).ToList();

            var restoreStockResult = await _catalogClient.RestoreStockAsync(stockItems, cancellationToken);
            if (!restoreStockResult.IsSuccess)
                return Result<Unit>.Success(Unit.Value);

            var cancelPaymentResult = payment.MarkAsCancelled();
            if (cancelPaymentResult.IsFailure)
                return Result<Unit>.Success(Unit.Value);

            var cancelOrderResult = order.MarkAsCancelled();
            if (cancelOrderResult.IsFailure)
                return Result<Unit>.Success(Unit.Value);
        }

        paymentModel.ApplyDomainState(payment);
        orderModel.Status = order.Status;

        if (data.Code == "00")
        {
            await _messagePublisher.PublishAsync(
                new OrderInvoiceEmailRequested
                {
                    UserId = orderModel.UserId,
                    OrderCode = orderModel.OrderCode,
                    TotalAmount = orderModel.TotalAmount,
                    CreatedAt = orderModel.CreatedAt,
                    Items = orderModel.Items.Select(item => new OrderInvoiceItemMessage
                    {
                        ProductName = item.Name,
                        Quantity = item.Quantity,
                        Price = item.Price
                    }).ToList()
                },
                cancellationToken);
        }

        return Result<Unit>.Success(Unit.Value);
    }
}
