using Platform.Domain.Common;
using Platform.Ordering.API.Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Platform.Ordering.API.Infrastructure.Persistence.Models;

[Table("Orders")]
public sealed class OrderModel : Entity
{
    public Guid UserId { get; set; }
    public long OrderCode { get; set; }
    public long TotalAmount { get; set; }
    public DateTime ExpiredAt { get; set; }
    public OrderStatus Status { get; set; }
    public ICollection<OrderItemModel> Items { get; set; } = [];
}
