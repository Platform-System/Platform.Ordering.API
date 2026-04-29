using Platform.Domain.Common;
using Platform.Ordering.API.Domain.Errors;

namespace Platform.Ordering.API.Domain.Entities;

public sealed class CartItem : Entity
{
    public Guid CartId { get; private set; }
    public Guid ProductId { get; private set; }
    public string Name { get; private set; } = null!;
    public long Price { get; private set; }
    public int Quantity { get; private set; }

    private CartItem()
    {
    }

    public DomainResult Initialize(Guid cartId, Guid productId, string name, long price, int quantity)
    {
        if (string.IsNullOrWhiteSpace(name))
            return DomainResult.Failure(OrderingErrors.Cart.InvalidItemName);

        if (price < 0)
            return DomainResult.Failure(OrderingErrors.Cart.InvalidItemPrice);

        if (quantity <= 0)
            return DomainResult.Failure(OrderingErrors.Cart.InvalidItemQuantity);

        CartId = cartId;
        ProductId = productId;
        Name = name;
        Price = price;
        Quantity = quantity;
        return DomainResult.Success();
    }

    public DomainResult IncreaseQuantity(int quantity)
    {
        if (quantity <= 0)
            return DomainResult.Failure(OrderingErrors.Cart.InvalidItemQuantity);

        Quantity += quantity;
        return DomainResult.Success();
    }

    public DomainResult UpdateQuantity(int quantity)
    {
        if (quantity <= 0)
            return DomainResult.Failure(OrderingErrors.Cart.InvalidItemQuantity);

        Quantity = quantity;
        return DomainResult.Success();
    }

    public static DomainResult<CartItem> Create(Guid cartId, Guid productId, string name, long price, int quantity)
    {
        var item = new CartItem();
        var result = item.Initialize(cartId, productId, name, price, quantity);
        return result.IsFailure
            ? DomainResult<CartItem>.Failure(result.Error)
            : DomainResult<CartItem>.Success(item);
    }
}
