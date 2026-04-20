using Platform.SharedKernel.Enums;

namespace Platform.Ordering.API.Application.Features.Carts.Shared;

public sealed class CartItemResponse
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public ProductKind Type { get; set; }
    public string Name { get; set; } = null!;
    public long Price { get; set; }
    public int Quantity { get; set; }
}
