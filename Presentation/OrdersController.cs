using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Platform.BuildingBlocks.Responses;
using Platform.Ordering.API.Application.Features.Orders.Commands.Checkout;
using Platform.Ordering.API.Application.Features.Orders.Commands.Webhook;

namespace Platform.Ordering.API.Presentation;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public sealed class OrdersController : ControllerBase
{
    private readonly ISender _mediator;

    public OrdersController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("{orderCode:long}/checkout")]
    public async Task<IActionResult> Checkout(long orderCode, CancellationToken cancellationToken)
    {
        var command = new CheckoutOrderCommand(orderCode);
        var result = await _mediator.Send(command, cancellationToken);
        return result.ToActionResult();
    }

    [AllowAnonymous]
    [HttpPost("webhook")]
    public async Task<IActionResult> Webhook(CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(Request.Body);
        var rawBody = await reader.ReadToEndAsync(cancellationToken);

        var command = new ProcessWebhookCommand(new WebhookRequest
        {
            RawBody = rawBody
        });

        await _mediator.Send(command, cancellationToken);
        return Ok();
    }
}
