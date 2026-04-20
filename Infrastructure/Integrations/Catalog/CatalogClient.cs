using Microsoft.Extensions.Options;
using Platform.BuildingBlocks.Responses;
using Platform.Ordering.API.Application.Abstractions.Integrations.Catalog;
using Platform.Catalog.Grpc;

namespace Platform.Ordering.API.Infrastructure.Integrations.Catalog;

public sealed class CatalogClient : ICatalogClient
{
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

        // Ordering gọi Catalog qua gRPC để lấy dữ liệu product phục vụ
        // rule của cart, đồng thời vẫn tách biệt OrderingDb với CatalogDb.
        return await GrpcIntegrationExecutor.ExecuteAsync(
            CatalogIntegrationDefinition.Name,
            async token => await _client.GetProductCartSnapshotAsync(
                new GetProductCartSnapshotRequest
                {
                    ProductId = productId.ToString()
                },
                cancellationToken: token),
            response => response.ToProductCartSnapshotResult(),
            cancellationToken);
    }
}
