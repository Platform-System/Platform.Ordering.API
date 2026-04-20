namespace Platform.Ordering.API.Application.Features.Carts.Commands.Add;

public sealed class AddToCartRequest
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}
