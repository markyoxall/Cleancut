using FluentValidation;

namespace CleanCut.Application.Commands.Orders.AddOrderLineItem;

/// <summary>
/// Validator for AddOrderLineItemCommand
/// </summary>
public class AddOrderLineItemCommandValidator : AbstractValidator<AddOrderLineItemCommand>
{
    public AddOrderLineItemCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty()
            .WithMessage("Order ID is required");

        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("Product ID is required");

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("Quantity must be greater than 0")
            .LessThanOrEqualTo(1000)
            .WithMessage("Quantity cannot exceed 1000");
    }
}