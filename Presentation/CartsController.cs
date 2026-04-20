using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Platform.BuildingBlocks.Responses;
using Platform.Ordering.API.Application.Features.Carts.Commands.Add;

namespace Platform.Ordering.API.Presentation;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public sealed class CartsController : ControllerBase
{
    private readonly ISender _mediator;

    public CartsController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("add")]
    public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request, CancellationToken cancellation)
    {
        var command = new AddToCartCommand(request);
        var result = await _mediator.Send(command, cancellation);
        return result.ToActionResult();
    }
}
