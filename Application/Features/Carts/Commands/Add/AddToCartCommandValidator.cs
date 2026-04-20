using FluentValidation;

namespace Platform.Ordering.API.Application.Features.Carts.Commands.Add;

public sealed class AddToCartCommandValidator : AbstractValidator<AddToCartCommand>
{
    public AddToCartCommandValidator()
    {
        RuleFor(x => x.Request.ProductId)
            .NotEmpty()
            .WithMessage("ProductId is required.");

        RuleFor(x => x.Request.Quantity)
            .GreaterThan(0)
            .WithMessage("Quantity must be greater than 0.")
            .LessThanOrEqualTo(100)
            .WithMessage("Quantity cannot exceed 100.");
    }
}
