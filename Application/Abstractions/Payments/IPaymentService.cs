using Platform.Ordering.API.Domain.Entities;
using Platform.Ordering.API.Application.Common.Models.Payment;
using Platform.Ordering.API.Application.Features.Orders.Commands.Webhook;

namespace Platform.Ordering.API.Application.Abstractions.Payments;

public interface IPaymentService
{
    Task<PaymentLinkResponse> CreatePaymentLink(Order order, CancellationToken cancellationToken = default);
    Task<WebhookRequest?> VerifyWebhook(string rawBody);
}
