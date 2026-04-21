namespace Platform.Ordering.API.Application.Abstractions.Integrations.Catalog;

public sealed class StockDeductionItem
{
    public Guid ProductId { get; init; }
    public int Quantity { get; init; }
}
