using FluentValidation;

namespace CleanCut.Application.Commands.Orders.DeleteOrder;

/// <summary>
/// Validator for DeleteOrderCommand
/// </summary>
public class DeleteOrderCommandValidator : AbstractValidator<DeleteOrderCommand>
{
    public DeleteOrderCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Order ID is required");
    }
}