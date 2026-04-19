using Platform.Domain.Common;
using Platform.Ordering.API.Domain.Errors;
using Platform.SharedKernel.Enums;

namespace Platform.Ordering.API.Domain.Entities;

public sealed class Cart : AggregateRoot
{
    public Guid UserId { get; private set; }

    private readonly List<CartItem> _items = [];
    public IReadOnlyCollection<CartItem> Items => _items.AsReadOnly();

    private Cart()
    {
    }

    public Cart(Guid userId)
    {
        UserId = userId;
    }

    public DomainResult AddItem(Guid productId, ProductKind type, string name, long price, int quantity)
    {
        var existingItem = Items.FirstOrDefault(x => x.ProductId == productId && x.Type == type);

        if (existingItem is not null)
        {
            return existingItem.IncreaseQuantity(quantity);
        }

        var itemResult = CartItem.Create(Id, productId, type, name, price, quantity);
        if (itemResult.IsFailure)
            return DomainResult.Failure(itemResult.Error);

        _items.Add(itemResult.Value);
        return DomainResult.Success();
    }

    public DomainResult AddItem(CartItem item)
    {
        if (item.CartId != Id)
            return DomainResult.Failure(OrderingErrors.Cart.ItemDoesNotBelongToCart);

        var existingItem = Items.FirstOrDefault(x => x.ProductId == item.ProductId && x.Type == item.Type);
        if (existingItem is not null)
        {
            return existingItem.IncreaseQuantity(item.Quantity);
        }

        _items.Add(item);
        return DomainResult.Success();
    }

    public DomainResult UpdateItem(Guid productId, ProductKind type, int quantity)
    {
        var item = Items.FirstOrDefault(x => x.ProductId == productId && x.Type == type);
        if (item is null)
            return DomainResult.Failure(OrderingErrors.Cart.ItemNotFound);

        return item.UpdateQuantity(quantity);
    }

    public DomainResult RemoveItem(Guid productId, ProductKind type)
    {
        var item = Items.FirstOrDefault(x => x.ProductId == productId && x.Type == type);
        if (item is null)
            return DomainResult.Failure(OrderingErrors.Cart.ItemNotFound);

        _items.Remove(item);
        return DomainResult.Success();
    }

    public long GetTotalAmount()
    {
        return _items.Sum(x => x.Price * x.Quantity);
    }

    public void Clear()
    {
        _items.Clear();
    }
}
