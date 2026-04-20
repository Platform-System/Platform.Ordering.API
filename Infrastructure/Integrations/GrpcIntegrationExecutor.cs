using Grpc.Core;
using Platform.BuildingBlocks.Responses;

namespace Platform.Ordering.API.Infrastructure.Integrations;

public static class GrpcIntegrationExecutor
{
    public static async Task<IntegrationResult<TValue>> ExecuteAsync<TResponse, TValue>(
        string integrationName,
        Func<CancellationToken, Task<TResponse>> operation,
        Func<TResponse, IntegrationResult<TValue>> mapResponse,
        CancellationToken cancellationToken)
    {
        try
        {
            // Gom lỗi transport của gRPC về một chỗ để từng client
            // chỉ cần lo phần request/response mapping.
            var response = await operation(cancellationToken);
            return mapResponse(response);
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable)
        {
            return IntegrationResult<TValue>.Failure(
                IntegrationErrorType.Unavailable,
                $"{integrationName} service is unavailable.");
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.Unauthenticated || ex.StatusCode == StatusCode.PermissionDenied)
        {
            return IntegrationResult<TValue>.Failure(
                IntegrationErrorType.Unauthorized,
                $"{integrationName} service rejected the request.");
        }
        catch (Exception)
        {
            return IntegrationResult<TValue>.Failure(
                IntegrationErrorType.Unknown,
                $"{integrationName} integration failed.");
        }
    }
}
