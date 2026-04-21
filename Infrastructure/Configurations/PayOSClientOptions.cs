namespace Platform.Ordering.API.Infrastructure.Configurations;

public class PayOSClientOptions
{
    public string ClientId { get; set; } = default!;
    public string ApiKey { get; set; } = default!;
    public string ChecksumKey { get; set; } = default!;
}
