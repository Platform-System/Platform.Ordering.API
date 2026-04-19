using Platform.Domain.Common;
using Platform.SharedKernel.Enums;

namespace Platform.Ordering.API.Domain.Entities;

public sealed class OrderItem : Entity
{
    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public ProductKind Type { get; private set; }
    public string Name { get; private set; } = null!;
    public long Price { get; private set; }
    public int Quantity { get; private set; }

    private OrderItem()
    {
    }

    public OrderItem(Guid orderId, Guid productId, ProductKind type, string name, long price, int quantity)
    {
        OrderId = orderId;
        ProductId = productId;
        Type = type;
        Name = name;
        Price = price;
        Quantity = quantity;
    }

    public void IncreaseQuantity(int quantity)
    {
        Quantity += quantity;
    }
}
