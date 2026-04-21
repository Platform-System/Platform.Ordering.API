using Platform.BuildingBlocks.Responses;

namespace Platform.Ordering.API.Application.Abstractions.Integrations.Catalog;

public interface ICatalogClient
{
    Task<IntegrationResult<ProductCartSnapshot>> GetProductCartSnapshotAsync(Guid productId, CancellationToken cancellationToken);
    Task<IntegrationResult<bool>> DecreaseStockAsync(IEnumerable<StockAdjustmentItem> items, CancellationToken cancellationToken);
    Task<IntegrationResult<bool>> RestoreStockAsync(IEnumerable<StockAdjustmentItem> items, CancellationToken cancellationToken);
}
