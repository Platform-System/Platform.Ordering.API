namespace Platform.Ordering.API.Infrastructure.Configurations;

public class PaymentSettings
{
    public string Provider { get; set; } = string.Empty;
    public string ReturnUrl { get; set; } = string.Empty;
    public string CancelUrl { get; set; } = string.Empty;
}
