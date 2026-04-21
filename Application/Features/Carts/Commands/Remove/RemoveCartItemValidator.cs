using FluentValidation;

namespace Platform.Ordering.API.Application.Features.Carts.Commands.Remove;

public sealed class RemoveCartItemValidator : AbstractValidator<RemoveCartItemCommand>
{
    public RemoveCartItemValidator()
    {
        RuleFor(x => x.Request.ProductId)
            .NotEmpty()
            .WithMessage("ProductId is required.");
    }
}
