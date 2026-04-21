using Platform.Application.Abstractions.Data;
using Platform.Application.Messaging;
using Platform.BuildingBlocks.Responses;
using Platform.Ordering.API.Application.Abstractions.Integrations.Catalog;
using Platform.Ordering.API.Application.Features.Carts.Shared;
using Platform.Ordering.API.Infrastructure.Persistence.Models;
using Platform.SharedKernel.Enums;
using Platform.SystemContext.Abstractions;

namespace Platform.Ordering.API.Application.Features.Carts.Commands.Update;

public sealed class UpdateCartItemHandler : ICommandHandler<UpdateCartItemCommand, CartResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICatalogClient _catalogClient;
    private readonly IUserContext _userContext;

    public UpdateCartItemHandler(
        IUnitOfWork unitOfWork,
        ICatalogClient catalogClient,
        IUserContext userContext)
    {
        _unitOfWork = unitOfWork;
        _catalogClient = catalogClient;
        _userContext = userContext;
    }

    public async Task<Result<CartResponse>> Handle(UpdateCartItemCommand command, CancellationToken cancellationToken)
    {
        if (_userContext.UserId is not Guid userId)
            return Result<CartResponse>.Failure("Unauthorized.");

        var cartRepository = _unitOfWork.GetRepository<CartModel>();
        var cartModel = await cartRepository.FindAsync(
            x => x.UserId == userId,
            false,
            cancellationToken,
            x => x.Items);

        if (cartModel is null)
            return Result<CartResponse>.Failure("Cart not found.");

        var cart = cartModel.ToDomain();
        var existingItem = cart.Items.FirstOrDefault(x => x.ProductId == command.Request.ProductId);

        if (existingItem is null)
            return Result<CartResponse>.Failure("Item not found in cart.");

        if (command.Request.NewQuantity == 0)
        {
            var removeResult = cart.RemoveItem(existingItem.ProductId, existingItem.Type);
            if (removeResult.IsFailure)
                return Result<CartResponse>.Failure("Unable to remove item from cart.");

            cart.UpdateModel(cartModel);
            return Result<CartResponse>.Success(cart.ToResponse());
        }

        var productResult = await _catalogClient.GetProductCartSnapshotAsync(command.Request.ProductId, cancellationToken);
        if (!productResult.IsSuccess)
        {
            return productResult.ErrorType switch
            {
                IntegrationErrorType.NotFound => Result<CartResponse>.Failure("Product not found."),
                IntegrationErrorType.InvalidConfiguration => Result<CartResponse>.Failure(productResult.Error ?? "Catalog integration is not configured."),
                IntegrationErrorType.Unavailable => Result<CartResponse>.Failure(productResult.Error ?? "Catalog service is unavailable."),
                IntegrationErrorType.Unauthorized => Result<CartResponse>.Failure(productResult.Error ?? "Catalog service rejected the request."),
                _ => Result<CartResponse>.Failure(productResult.Error ?? "Catalog integration failed.")
            };
        }

        var product = productResult.Value;
        if (product is null || !product.IsActive)
            return Result<CartResponse>.Failure("Product not found.");

        if (product.Type == ProductKind.DigitalProduct && command.Request.NewQuantity > 1)
            return Result<CartResponse>.Failure("Cannot have more than 1 quantity of a digital product.");

        if (product.Stock.HasValue && command.Request.NewQuantity > product.Stock.Value)
            return Result<CartResponse>.Failure("Not enough stock available.");

        var updateResult = cart.UpdateItem(product.Id, product.Type, command.Request.NewQuantity);
        if (updateResult.IsFailure)
            return Result<CartResponse>.Failure("Unable to update cart item.");

        cart.UpdateModel(cartModel);

        return Result<CartResponse>.Success(cart.ToResponse());
    }
}
