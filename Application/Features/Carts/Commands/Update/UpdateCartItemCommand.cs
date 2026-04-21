using Platform.Application.Messaging;
using Platform.Ordering.API.Application.Features.Carts.Shared;

namespace Platform.Ordering.API.Application.Features.Carts.Commands.Update;

public sealed class UpdateCartItemCommand : ICommand<CartResponse>
{
    public UpdateCartItemRequest Request { get; }

    public UpdateCartItemCommand(UpdateCartItemRequest request)
    {
        Request = request;
    }
}
