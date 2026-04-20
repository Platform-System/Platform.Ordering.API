using Platform.Application.Messaging;
using Platform.Ordering.API.Application.Features.Carts.Shared;

namespace Platform.Ordering.API.Application.Features.Carts.Commands.Add;

public sealed class AddToCartCommand(AddToCartRequest request) : ICommand<CartResponse>
{
    public AddToCartRequest Request { get; } = request;
}
