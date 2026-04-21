using Platform.Application.Abstractions.Data;
using Platform.Application.Messaging;
using Platform.BuildingBlocks.Responses;
using Platform.Ordering.API.Application.Features.Carts.Shared;
using Platform.Ordering.API.Infrastructure.Persistence.Models;
using Platform.SystemContext.Abstractions;

namespace Platform.Ordering.API.Application.Features.Carts.Queries.GetCartByUserId;

public sealed class GetCartByUserIdHandler : IQueryHandler<GetCartByUserIdQuery, CartResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserContext _userContext;

    public GetCartByUserIdHandler(IUnitOfWork unitOfWork, IUserContext userContext)
    {
        _unitOfWork = unitOfWork;
        _userContext = userContext;
    }

    public async Task<Result<CartResponse>> Handle(GetCartByUserIdQuery query, CancellationToken cancellationToken)
    {
        if (_userContext.UserId is not Guid userId)
            return Result<CartResponse>.Failure("Unauthorized.");

        var cartModel = await _unitOfWork.GetRepository<CartModel>().FindAsync(
            x => x.UserId == userId,
            true,
            cancellationToken,
            x => x.Items);

        if (cartModel is null)
            return Result<CartResponse>.Failure("Cart not found.");

        var cart = cartModel.ToDomain();
        if (!cart.Items.Any())
            return Result<CartResponse>.Failure("Cart is empty.");

        return Result<CartResponse>.Success(cart.ToResponse());
    }
}
