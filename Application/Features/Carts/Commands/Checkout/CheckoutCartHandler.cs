using Platform.Application.Abstractions.Data;
using Platform.Application.Messaging;
using Platform.BuildingBlocks.Responses;
using Platform.Ordering.API.Application.Abstractions.Integrations.Catalog;
using Platform.Ordering.API.Application.Features.Carts.Shared;
using Platform.Ordering.API.Application.Features.Orders.Shared;
using Platform.Ordering.API.Domain.Entities;
using Platform.Ordering.API.Infrastructure.Persistence.Models;
using Platform.SystemContext.Abstractions;

namespace Platform.Ordering.API.Application.Features.Carts.Commands.Checkout;

public sealed class CheckoutCartHandler : ICommandHandler<CheckoutCartCommand, OrderResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICatalogClient _catalogClient;
    private readonly IUserContext _userContext;

    public CheckoutCartHandler(
        IUnitOfWork unitOfWork,
        ICatalogClient catalogClient,
        IUserContext userContext)
    {
        _unitOfWork = unitOfWork;
        _catalogClient = catalogClient;
        _userContext = userContext;
    }

    public async Task<Result<OrderResponse>> Handle(CheckoutCartCommand command, CancellationToken cancellationToken)
    {
        if (_userContext.UserId is not Guid userId)
            return Result<OrderResponse>.Failure("Unauthorized.");

        var cartRepository = _unitOfWork.GetRepository<CartModel>();
        var orderRepository = _unitOfWork.GetRepository<OrderModel>();

        var cartModel = await cartRepository.FindAsync(
            x => x.UserId == userId,
            false,
            cancellationToken,
            x => x.Items);

        if (cartModel is null)
            return Result<OrderResponse>.Failure("Cart not found.");

        var cart = cartModel.ToDomain();
        if (!cart.Items.Any())
            return Result<OrderResponse>.Failure("Cart is empty.");

        // Checkout không đụng thẳng CatalogDb. Ordering chỉ gửi danh sách cần trừ kho
        // sang Catalog để service sở hữu dữ liệu stock tự xử lý.
        var decreaseStockItems = cart.Items.Select(item => new StockAdjustmentItem
        {
            ProductId = item.ProductId,
            Quantity = item.Quantity
        }).ToList();

        var decreaseStockResult = await _catalogClient.DecreaseStockAsync(decreaseStockItems, cancellationToken);
        if (!decreaseStockResult.IsSuccess)
        {
            return decreaseStockResult.ErrorType switch
            {
                IntegrationErrorType.NotFound => Result<OrderResponse>.Failure("Product not found."),
                IntegrationErrorType.InvalidConfiguration => Result<OrderResponse>.Failure(decreaseStockResult.Error ?? "Catalog integration is not configured."),
                IntegrationErrorType.Unavailable => Result<OrderResponse>.Failure(decreaseStockResult.Error ?? "Catalog service is unavailable."),
                IntegrationErrorType.Unauthorized => Result<OrderResponse>.Failure(decreaseStockResult.Error ?? "Catalog service rejected the request."),
                _ => Result<OrderResponse>.Failure(decreaseStockResult.Error ?? "Catalog integration failed.")
            };
        }

        var order = new Order(userId);
        // Sau khi Catalog xác nhận trừ stock thành công, Ordering mới chốt order
        // từ dữ liệu cart hiện tại và lưu vào OrderingDb.
        foreach (var cartItem in cart.Items)
        {
            var addItemResult = order.AddItem(cartItem.ProductId, cartItem.Name, cartItem.Price, cartItem.Quantity);
            if (addItemResult.IsFailure)
                return Result<OrderResponse>.Failure("Unable to create order item.");
        }

        await orderRepository.AddAsync(order.ToModel(), cancellationToken);

        cart.Clear();
        cart.UpdateModel(cartModel);

        return Result<OrderResponse>.Success(order.ToResponse());
    }
}
