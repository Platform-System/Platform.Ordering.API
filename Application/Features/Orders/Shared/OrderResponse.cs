using Platform.Ordering.API.Domain.Enums;

namespace Platform.Ordering.API.Application.Features.Orders.Shared;

public sealed class OrderResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public long OrderCode { get; set; }
    public long TotalAmount { get; set; }
    public DateTime ExpiredAt { get; set; }
    public OrderStatus Status { get; set; }
    public string? CheckoutUrl { get; set; }
    public List<OrderItemResponse> Items { get; set; } = [];
}
