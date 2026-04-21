using Platform.BuildingBlocks.Responses;

namespace Platform.Ordering.API.Application.Abstractions.Integrations.Catalog;

public interface ICatalogClient
{
    Task<IntegrationResult<ProductCartSnapshot>> GetProductCartSnapshotAsync(Guid productId, CancellationToken cancellationToken);
    Task<IntegrationResult<bool>> DecreaseStockAsync(IEnumerable<StockDeductionItem> items, CancellationToken cancellationToken);
}
