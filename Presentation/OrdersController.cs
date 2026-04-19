using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Platform.Ordering.API.Presentation;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public sealed class OrdersController : ControllerBase
{
    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new
        {
            Service = "Platform Ordering API",
            Domain = new[]
            {
                "Cart",
                "CartItem",
                "Order",
                "OrderItem",
                "Payment"
            }
        });
    }
}
