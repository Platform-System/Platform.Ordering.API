namespace Platform.Ordering.API.Application.Features.Orders.Shared;

public sealed class OrderItemResponse
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string Name { get; set; } = null!;
    public long Price { get; set; }
    public int Quantity { get; set; }
}
