using MediatR;
using Platform.Application.Messaging;

namespace Platform.Ordering.API.Application.Features.Orders.Commands.Webhook;

public sealed class ProcessWebhookCommand : ICommand
{
    public WebhookRequest Request { get; }

    public ProcessWebhookCommand(WebhookRequest request)
    {
        Request = request;
    }
}
