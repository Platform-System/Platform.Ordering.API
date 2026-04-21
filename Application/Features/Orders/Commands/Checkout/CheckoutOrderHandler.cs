using Platform.Application.Abstractions.Data;
using Platform.Application.Messaging;
using Platform.BuildingBlocks.Responses;
using Platform.Ordering.API.Application.Abstractions.Payments;
using Platform.Ordering.API.Application.Features.Orders.Shared;
using Platform.Ordering.API.Domain.Entities;
using Platform.Ordering.API.Domain.Enums;
using Platform.Ordering.API.Infrastructure.Persistence.Models;
using Platform.SystemContext.Abstractions;

namespace Platform.Ordering.API.Application.Features.Orders.Commands.Checkout;

public sealed class CheckoutOrderHandler : ICommandHandler<CheckoutOrderCommand, OrderResponse>
{
    private readonly IPaymentService _paymentService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserContext _userContext;

    public CheckoutOrderHandler(IPaymentService paymentService, IUnitOfWork unitOfWork, IUserContext userContext)
    {
        _paymentService = paymentService;
        _unitOfWork = unitOfWork;
        _userContext = userContext;
    }

    public async Task<Result<OrderResponse>> Handle(CheckoutOrderCommand command, CancellationToken cancellationToken)
    {
        if (_userContext.UserId is not Guid userId)
            return Result<OrderResponse>.Failure("Unauthorized.");

        var orderRepository = _unitOfWork.GetRepository<OrderModel>();
        var paymentRepository = _unitOfWork.GetRepository<PaymentModel>();

        var orderModel = await orderRepository.FindAsync(
            x => x.OrderCode == command.OrderCode,
            false,
            cancellationToken,
            x => x.Items);

        if (orderModel is null)
            return Result<OrderResponse>.Failure("Order not found.");

        var order = orderModel.ToDomain();

        if (order.UserId != userId)
            return Result<OrderResponse>.Failure("Order not found.");

        if (order.Status == OrderStatus.Paid)
            return Result<OrderResponse>.Failure("Order already paid.");

        if (order.Status == OrderStatus.Cancelled)
            return Result<OrderResponse>.Failure("Order cancelled.");

        var existingPaymentModel = await paymentRepository.FindAsync(
            x => x.OrderId == order.Id && x.Status == PaymentStatus.Pending,
            false,
            cancellationToken);

        if (existingPaymentModel is not null)
        {
            var existingPayment = existingPaymentModel.ToDomain();

            if (!string.IsNullOrWhiteSpace(existingPayment.CheckoutUrl))
                return Result<OrderResponse>.Success(order.ToResponse(existingPayment.CheckoutUrl));
        }

        var payment = existingPaymentModel?.ToDomain() ?? new Payment(order.Id);
        var addPaymentResult = order.AddPayment(payment);
        if (addPaymentResult.IsFailure)
            return Result<OrderResponse>.Failure("Unable to attach payment to order.");

        var paymentLink = await _paymentService.CreatePaymentLink(order, cancellationToken);

        if (string.IsNullOrWhiteSpace(paymentLink.CheckoutUrl)
            || string.IsNullOrWhiteSpace(paymentLink.PaymentLinkId)
            || string.IsNullOrWhiteSpace(paymentLink.Currency))
        {
            return Result<OrderResponse>.Failure("Unable to create a payment link.");
        }

        var setCheckoutResult = payment.SetCheckout(
            paymentLink.CheckoutUrl,
            paymentLink.PaymentLinkId,
            paymentLink.Amount,
            paymentLink.Currency);

        if (setCheckoutResult.IsFailure)
            return Result<OrderResponse>.Failure("Unable to update payment checkout information.");

        if (existingPaymentModel is null)
        {
            await paymentRepository.AddAsync(payment.ToModel(), cancellationToken);
        }
        else
        {
            existingPaymentModel.ApplyDomainState(payment);
        }

        return Result<OrderResponse>.Success(order.ToResponse(payment.CheckoutUrl));
    }
}
