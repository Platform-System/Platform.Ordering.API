namespace Platform.Ordering.API.Application.Abstractions.Integrations.Catalog;

public sealed class ProductCartSnapshot
{
    public Guid Id { get; init; }
    public string Title { get; init; } = null!;
    public long Price { get; init; }
    public bool IsActive { get; init; }
    public int? Stock { get; init; }
}
