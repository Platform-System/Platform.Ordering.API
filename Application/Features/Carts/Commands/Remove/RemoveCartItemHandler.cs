using Platform.Application.Abstractions.Data;
using Platform.Application.Messaging;
using Platform.BuildingBlocks.Responses;
using Platform.Ordering.API.Application.Features.Carts.Shared;
using Platform.Ordering.API.Infrastructure.Persistence.Models;
using Platform.SystemContext.Abstractions;

namespace Platform.Ordering.API.Application.Features.Carts.Commands.Remove;

public sealed class RemoveCartItemHandler : ICommandHandler<RemoveCartItemCommand, CartResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserContext _userContext;

    public RemoveCartItemHandler(IUnitOfWork unitOfWork, IUserContext userContext)
    {
        _unitOfWork = unitOfWork;
        _userContext = userContext;
    }

    public async Task<Result<CartResponse>> Handle(RemoveCartItemCommand command, CancellationToken cancellationToken)
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

        var removeResult = cart.RemoveItem(existingItem.ProductId);
        if (removeResult.IsFailure)
            return Result<CartResponse>.Failure("Unable to remove item from cart.");

        cart.UpdateModel(cartModel);

        return Result<CartResponse>.Success(cart.ToResponse());
    }
}
