using Platform.Application.Messaging;
using Platform.Ordering.API.Application.Features.Carts.Shared;

namespace Platform.Ordering.API.Application.Features.Carts.Commands.Remove;

public sealed class RemoveCartItemCommand : ICommand<CartResponse>
{
    public RemoveCartItemRequest Request { get; }

    public RemoveCartItemCommand(RemoveCartItemRequest request)
    {
        Request = request;
    }
}
