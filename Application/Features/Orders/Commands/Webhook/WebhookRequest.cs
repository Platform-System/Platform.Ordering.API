namespace Platform.Ordering.API.Application.Features.Orders.Commands.Webhook;

public class WebhookRequest
{
    public long OrderCode { get; set; }
    public string Code { get; set; } = default!;
    public string Signature { get; set; } = default!;
    public string RawBody { get; set; } = default!;
}
