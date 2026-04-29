using Platform.Domain.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace Platform.Ordering.API.Infrastructure.Persistence.Models;

[Table("OrderItems")]
public sealed class OrderItemModel : Entity
{
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
    public string Name { get; set; } = null!;
    public long Price { get; set; }
    public int Quantity { get; set; }

    [ForeignKey(nameof(OrderId))]
    public OrderModel Order { get; set; } = null!;
}
