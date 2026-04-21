using Microsoft.Extensions.Options;
using Platform.BuildingBlocks.Responses;
using Platform.Ordering.API.Application.Abstractions.Integrations.Catalog;
using Platform.Catalog.Grpc;

namespace Platform.Ordering.API.Infrastructure.Integrations.Catalog;

public sealed class CatalogClient : ICatalogClient
{
    // _client là gRPC client được generate từ file proto `catalog_integration.proto`
    // và được inject qua DI để Ordering gọi sang Catalog.
    private readonly CatalogIntegration.CatalogIntegrationClient _client;
    private readonly CatalogClientOptions _options;

    public CatalogClient(CatalogIntegration.CatalogIntegrationClient client, IOptions<CatalogClientOptions> options)
    {
        _client = client;
        _options = options.Value;
    }

    public async Task<IntegrationResult<ProductCartSnapshot>> GetProductCartSnapshotAsync(Guid productId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.Address))
        {
            return IntegrationResult<ProductCartSnapshot>.Failure(
                IntegrationErrorType.InvalidConfiguration,
                $"{CatalogIntegrationDefinition.Name} integration address is not configured.");
        }

        // operation là hàm thực sự gọi gRPC sang Catalog.
        // Lambda này nhận CancellationToken từ executor, còn productId được lấy
        // từ biến của method hiện tại để đưa vào request gửi sang service đích.
        Func<CancellationToken, Task<GetProductCartSnapshotResponse>> operation = async token =>
            await _client.GetProductCartSnapshotAsync(
                new GetProductCartSnapshotRequest
                {
                    ProductId = productId.ToString()
                },
                cancellationToken: token);

        // mapResponse dùng để đổi kiểu dữ liệu:
        // - trước map: GetProductCartSnapshotResponse (DTO gRPC từ Catalog)
        // - sau map: IntegrationResult<ProductCartSnapshot> (kiểu Ordering dùng)
        // Nhờ vậy application layer không phải làm việc trực tiếp với gRPC DTO.
        Func<GetProductCartSnapshotResponse, IntegrationResult<ProductCartSnapshot>> mapResponse =
            response => response.ToProductCartSnapshotResult();

        // Ordering gọi Catalog qua gRPC để lấy dữ liệu product phục vụ
        // rule của cart, đồng thời vẫn tách biệt OrderingDb với CatalogDb.
        return await GrpcIntegrationExecutor.ExecuteAsync(
            CatalogIntegrationDefinition.Name,
            operation,
            mapResponse,
            cancellationToken);
    }

    public async Task<IntegrationResult<bool>> DecreaseStockAsync(IEnumerable<StockDeductionItem> items, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.Address))
        {
            return IntegrationResult<bool>.Failure(
                IntegrationErrorType.InvalidConfiguration,
                $"{CatalogIntegrationDefinition.Name} integration address is not configured.");
        }

        Func<CancellationToken, Task<DecreaseStockResponse>> operation = async token =>
            await _client.DecreaseStockAsync(
                new DecreaseStockRequest
                {
                    Items = { items.Select(item => new DecreaseStockItem
                    {
                        ProductId = item.ProductId.ToString(),
                        Quantity = item.Quantity
                    }) }
                },
                cancellationToken: token);

        Func<DecreaseStockResponse, IntegrationResult<bool>> mapResponse =
            response => response.ToDecreaseStockResult();

        return await GrpcIntegrationExecutor.ExecuteAsync(
            CatalogIntegrationDefinition.Name,
            operation,
            mapResponse,
            cancellationToken);
    }
}
