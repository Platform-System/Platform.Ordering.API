namespace Platform.Ordering.API.Application.Features.Carts.Commands.Remove;

public sealed class RemoveCartItemRequest
{
    public Guid ProductId { get; set; }
}
