namespace Platform.Ordering.API.Application.Features.Carts.Shared;

public sealed class CartResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public List<CartItemResponse> Items { get; set; } = [];
    public long TotalAmount { get; set; }
}
