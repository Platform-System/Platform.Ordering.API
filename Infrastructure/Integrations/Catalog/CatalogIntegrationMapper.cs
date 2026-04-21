using Platform.BuildingBlocks.Responses;
using Platform.Catalog.Grpc;
using Platform.Ordering.API.Application.Abstractions.Integrations.Catalog;
using Platform.SharedKernel.Enums;

namespace Platform.Ordering.API.Infrastructure.Integrations.Catalog;

public static class CatalogIntegrationMapper
{
    public static IntegrationResult<ProductCartSnapshot> ToProductCartSnapshotResult(this GetProductCartSnapshotResponse response)
    {
        // Lỗi nghiệp vụ từ Catalog được trả theo contract gRPC
        // và được đổi ở đây sang IntegrationResult cho application layer.
        if (!response.IsSuccess)
        {
            return response.ErrorCode switch
            {
                CatalogErrorCodeGrpc.ProductNotFound => IntegrationResult<ProductCartSnapshot>.Failure(
                    IntegrationErrorType.NotFound,
                    response.ErrorMessage ?? "Product not found."),
                CatalogErrorCodeGrpc.InvalidProductId => IntegrationResult<ProductCartSnapshot>.Failure(
                    IntegrationErrorType.Unknown,
                    response.ErrorMessage ?? "Invalid product id."),
                _ => IntegrationResult<ProductCartSnapshot>.Failure(
                    IntegrationErrorType.Unknown,
                    response.ErrorMessage ?? "Catalog integration failed.")
            };
        }

        return IntegrationResult<ProductCartSnapshot>.Success(new ProductCartSnapshot
        {
            Id = Guid.Parse(response.Id),
            Title = response.Title,
            Price = response.Price,
            Type = response.Kind == ProductKindGrpc.Physical
                ? ProductKind.PhysicalProduct
                : ProductKind.DigitalProduct,
            IsActive = response.IsActive,
            Stock = response.HasStock ? response.Stock : null
        });
    }

    public static IntegrationResult<bool> ToDecreaseStockResult(this DecreaseStockResponse response)
    {
        if (!response.IsSuccess)
        {
            return response.ErrorCode switch
            {
                CatalogErrorCodeGrpc.ProductNotFound => IntegrationResult<bool>.Failure(
                    IntegrationErrorType.NotFound,
                    response.ErrorMessage ?? "Product not found."),
                CatalogErrorCodeGrpc.InvalidQuantity => IntegrationResult<bool>.Failure(
                    IntegrationErrorType.Unknown,
                    response.ErrorMessage ?? "Invalid quantity."),
                CatalogErrorCodeGrpc.InsufficientStock => IntegrationResult<bool>.Failure(
                    IntegrationErrorType.Unknown,
                    response.ErrorMessage ?? "Not enough stock available."),
                _ => IntegrationResult<bool>.Failure(
                    IntegrationErrorType.Unknown,
                    response.ErrorMessage ?? "Catalog integration failed.")
            };
        }

        return IntegrationResult<bool>.Success(true);
    }
}
