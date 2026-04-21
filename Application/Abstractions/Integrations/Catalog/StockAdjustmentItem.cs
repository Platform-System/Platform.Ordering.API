namespace Platform.Ordering.API.Application.Abstractions.Integrations.Catalog;

public sealed class StockAdjustmentItem
{
    public Guid ProductId { get; init; }
    public int Quantity { get; init; }
}
