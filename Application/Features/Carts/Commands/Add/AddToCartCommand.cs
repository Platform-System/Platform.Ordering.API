using Platform.Application.Messaging;
using Platform.Ordering.API.Application.Features.Carts.Shared;

namespace Platform.Ordering.API.Application.Features.Carts.Commands.Add;

public sealed class AddToCartCommand : ICommand<CartResponse>
{
    public AddToCartRequest Request { get; }

    public AddToCartCommand(AddToCartRequest request)
    {
        Request = request;
    }
}
