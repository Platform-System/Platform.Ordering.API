using FluentValidation;

namespace Platform.Ordering.API.Application.Features.Carts.Commands.Update;

public sealed class UpdateCartItemValidator : AbstractValidator<UpdateCartItemCommand>
{
    public UpdateCartItemValidator()
    {
        RuleFor(x => x.Request.ProductId)
            .NotEmpty()
            .WithMessage("ProductId is required.");

        RuleFor(x => x.Request.NewQuantity)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Quantity must be greater than or equal to 0.")
            .LessThanOrEqualTo(100)
            .WithMessage("Quantity cannot exceed 100.");
    }
}
