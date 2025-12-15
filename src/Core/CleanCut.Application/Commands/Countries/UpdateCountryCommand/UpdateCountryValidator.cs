using FluentValidation;

namespace CleanCut.Application.Commands.Countries.UpdateCountryCommand;

/// <summary>
/// Validator for UpdateCountryCommand
/// </summary>
public class UpdateCountryCommandValidator : AbstractValidator<UpdateCountryCommand>
{
    public UpdateCountryCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Country ID is required");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Country name is required")
            .MaximumLength(100)
            .WithMessage("Country name cannot exceed 100 characters");

        RuleFor(x => x.Code)
            .NotEmpty()
            .WithMessage("Country code is required")
            .MaximumLength(10)
            .WithMessage("Country code cannot exceed 10 characters");
    }
}