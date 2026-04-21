namespace Platform.Ordering.API.Application.Features.Carts.Commands.Update;

public sealed class UpdateCartItemRequest
{
    public Guid ProductId { get; set; }
    public int NewQuantity { get; set; }
}
