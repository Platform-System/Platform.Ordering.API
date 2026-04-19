using Platform.Domain.Common;
using Platform.Ordering.API.Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Platform.Ordering.API.Infrastructure.Persistence.Models;

[Table("Payments")]
public sealed class PaymentModel : Entity
{
    public Guid OrderId { get; set; }
    public string? PaymentLinkId { get; set; }
    public string? CheckoutUrl { get; set; }
    public long Amount { get; set; }
    public string? Currency { get; set; }
    public DateTime? PaidAt { get; set; }
    public PaymentStatus Status { get; set; }

    [ForeignKey(nameof(OrderId))]
    public OrderModel Order { get; set; } = null!;
}
