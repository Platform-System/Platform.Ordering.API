using Platform.Application.Abstractions.Data;
using Platform.Application.Messaging;
using Platform.BuildingBlocks.Responses;
using Platform.Ordering.API.Application.Abstractions.Integrations.Catalog;
using Platform.Ordering.API.Application.Features.Carts.Shared;
using Platform.Ordering.API.Domain.Entities;
using Platform.Ordering.API.Infrastructure.Persistence.Models;
using Platform.SharedKernel.Enums;
using Platform.SystemContext.Abstractions;

namespace Platform.Ordering.API.Application.Features.Carts.Commands.Add;

public sealed class AddToCartHandler : ICommandHandler<AddToCartCommand, CartResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICatalogClient _catalogClient;
    private readonly IUserContext _userContext;

    public AddToCartHandler(IUnitOfWork unitOfWork, ICatalogClient catalogClient, IUserContext userContext)
    {
        _unitOfWork = unitOfWork;
        _catalogClient = catalogClient;
        _userContext = userContext;
    }

    public async Task<Result<CartResponse>> Handle(AddToCartCommand command, CancellationToken cancellationToken)
    {
        if (_userContext.UserId is not Guid userId)
            return Result<CartResponse>.Failure("Unauthorized.");

        var cartRepository = _unitOfWork.GetRepository<CartModel>();
        var cartModel = await cartRepository.FindAsync(
            x => x.UserId == userId,
            false,
            cancellationToken,
            x => x.Items);
        var cart = cartModel?.ToDomain() ?? new Cart(userId);

        // Ordering lấy snapshot product từ Catalog qua gRPC rồi mới áp
        // rule thêm vào giỏ hàng, thay vì đọc dữ liệu product trực tiếp từ DB khác.
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
        if (product is null)
            return Result<CartResponse>.Failure("Catalog integration returned an empty response.");

        if (!product.IsActive)
            return Result<CartResponse>.Failure("Product not found.");

        var existingItem = cart.Items.FirstOrDefault(x => x.ProductId == command.Request.ProductId);

        // Sau khi có snapshot từ Catalog, toàn bộ rule của cart sẽ được xử lý
        // trong Ordering trước khi lưu lại vào OrderingDb.
        if (existingItem is not null)
        {
            if (product.Type == ProductKind.DigitalProduct)
                return Result<CartResponse>.Failure("Cannot add multiple quantities of a digital product.");

            if (product.Stock.HasValue && existingItem.Quantity + command.Request.Quantity > product.Stock.Value)
                return Result<CartResponse>.Failure("Not enough stock available.");

            var addItemResult = cart.AddItem(product.Id, product.Type, product.Title, product.Price, command.Request.Quantity);
            if (addItemResult.IsFailure)
                return Result<CartResponse>.Failure("Unable to add item to cart.");
        }
        else
        {
            if (product.Type == ProductKind.DigitalProduct && command.Request.Quantity > 1)
                return Result<CartResponse>.Failure("Quantity must be 1 for digital products.");

            if (product.Stock.HasValue && command.Request.Quantity > product.Stock.Value)
                return Result<CartResponse>.Failure("Not enough stock available.");

            var addItemResult = cart.AddItem(product.Id, product.Type, product.Title, product.Price, command.Request.Quantity);
            if (addItemResult.IsFailure)
                return Result<CartResponse>.Failure("Unable to add item to cart.");
        }

        if (cartModel is null)
        {
            cartModel = cart.ToModel();
            await cartRepository.AddAsync(cartModel, cancellationToken);
        }
        else
        {
            cart.UpdateModel(cartModel);
        }

        return Result<CartResponse>.Success(cart.ToResponse());
    }
}
