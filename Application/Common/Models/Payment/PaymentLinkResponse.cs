using Platform.Ordering.API.Domain.Enums;

namespace Platform.Ordering.API.Application.Common.Models.Payment;

public class PaymentLinkResponse
{
    public string? CheckoutUrl { get; set; }
    public string? PaymentLinkId { get; set; }
    public long Amount { get; set; }
    public string? Currency { get; set; }
    public PaymentStatus Status { get; set; }
}
