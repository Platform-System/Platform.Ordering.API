using Grpc.Core;
using Platform.BuildingBlocks.Responses;

namespace Platform.Ordering.API.Infrastructure.Integrations;

public static class GrpcIntegrationExecutor
{
    // Wrapper dùng chung cho outbound gRPC call:
    // - operation: hàm thực sự gọi sang service khác
    // - mapResponse: hàm đổi response gRPC sang IntegrationResult của application
    // - phần lỗi transport sẽ được gom và đổi về IntegrationResult tại đây
    public static async Task<IntegrationResult<TValue>> ExecuteAsync<TResponse, TValue>(
        string integrationName,
        Func<CancellationToken, Task<TResponse>> operation,
        Func<TResponse, IntegrationResult<TValue>> mapResponse,
        CancellationToken cancellationToken)
    {
        try
        {
            // Nếu call thành công ở mức transport thì chỉ cần map response
            // sang kiểu dữ liệu mà application layer đang dùng.
            var response = await operation(cancellationToken);
            return mapResponse(response);
        }
        catch (Exception ex)
        {
            if (ex is RpcException rpcException)
            {
                return rpcException.StatusCode switch
                {
                    StatusCode.Unavailable => IntegrationResult<TValue>.Failure(
                        IntegrationErrorType.Unavailable,
                        $"{integrationName} service is unavailable."),
                    StatusCode.Unauthenticated or StatusCode.PermissionDenied => IntegrationResult<TValue>.Failure(
                        IntegrationErrorType.Unauthorized,
                        $"{integrationName} service rejected the request."),
                    _ => IntegrationResult<TValue>.Failure(
                        IntegrationErrorType.Unknown,
                        $"{integrationName} integration failed.")
                };
            }

            // Các lỗi còn lại của tầng integration được gom về một nhánh chung.
            return IntegrationResult<TValue>.Failure(
                IntegrationErrorType.Unknown,
                $"{integrationName} integration failed.");
        }
    }
}
