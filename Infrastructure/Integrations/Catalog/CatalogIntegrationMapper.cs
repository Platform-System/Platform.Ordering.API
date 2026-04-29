using Platform.BuildingBlocks.Responses;
using Platform.Catalog.Grpc;
using Platform.Common.Grpc;
using Platform.Ordering.API.Application.Abstractions.Integrations.Catalog;

namespace Platform.Ordering.API.Infrastructure.Integrations.Catalog;

public static class CatalogIntegrationMapper
{
    public static IntegrationResult<ProductCartSnapshot> ToProductCartSnapshotResult(this GetProductCartSnapshotResponse response)
    {
        // Lỗi nghiệp vụ từ Catalog được trả theo contract gRPC
        // và được đổi ở đây sang IntegrationResult cho application layer.
        if (response.Status.IsFailure())
        {
            var errorMessage = response.Status.GetFirstErrorOrDefault("Catalog integration failed.");
            var errorType = string.Equals(errorMessage, "Product not found.", StringComparison.OrdinalIgnoreCase)
                ? IntegrationErrorType.NotFound
                : IntegrationErrorType.Unknown;

            return IntegrationResult<ProductCartSnapshot>.Failure(errorType, errorMessage);
        }

        if (response.Data is null)
        {
            return IntegrationResult<ProductCartSnapshot>.Failure(
                IntegrationErrorType.Unknown,
                "Catalog integration returned empty data.");
        }

        return IntegrationResult<ProductCartSnapshot>.Success(new ProductCartSnapshot
        {
            Id = Guid.Parse(response.Data.Id),
            Title = response.Data.Title,
            Price = response.Data.Price,
            IsActive = response.Data.IsActive,
            Stock = response.Data.Stock
        });
    }

    public static IntegrationResult<bool> ToAdjustStockResult(this AdjustStockResponse response)
    {
        if (response.Status.IsFailure())
        {
            var errorMessage = response.Status.GetFirstErrorOrDefault("Catalog integration failed.");
            var errorType = string.Equals(errorMessage, "Product not found.", StringComparison.OrdinalIgnoreCase)
                ? IntegrationErrorType.NotFound
                : IntegrationErrorType.Unknown;

            return IntegrationResult<bool>.Failure(errorType, errorMessage);
        }

        return IntegrationResult<bool>.Success(true);
    }
}
