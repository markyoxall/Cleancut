using FluentValidation;

namespace CleanCut.Application.Commands.Orders.UpdateOrder;

/// <summary>
/// Validator for UpdateOrderCommand
/// </summary>
public class UpdateOrderCommandValidator : AbstractValidator<UpdateOrderCommand>
{
    public UpdateOrderCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Order ID is required");

        RuleFor(x => x.ShippingAddress)
            .NotEmpty()
            .WithMessage("Shipping address is required")
            .MaximumLength(500)
            .WithMessage("Shipping address cannot exceed 500 characters");

        RuleFor(x => x.BillingAddress)
            .NotEmpty()
            .WithMessage("Billing address is required")
            .MaximumLength(500)
            .WithMessage("Billing address cannot exceed 500 characters");

        RuleFor(x => x.Notes)
            .MaximumLength(1000)
            .WithMessage("Notes cannot exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}