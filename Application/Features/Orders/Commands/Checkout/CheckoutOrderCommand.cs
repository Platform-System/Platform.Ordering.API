using Platform.Application.Messaging;
using Platform.Ordering.API.Application.Features.Orders.Shared;

namespace Platform.Ordering.API.Application.Features.Orders.Commands.Checkout;

public sealed class CheckoutOrderCommand : ICommand<OrderResponse>
{
    public long OrderCode { get; }

    public CheckoutOrderCommand(long orderCode)
    {
        OrderCode = orderCode;
    }
}
