using FluentValidation;

namespace CleanCut.Application.Commands.Orders.CreateOrder;

/// <summary>
/// Validator for CreateOrderCommand
/// </summary>
public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .WithMessage("Customer ID is required");

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