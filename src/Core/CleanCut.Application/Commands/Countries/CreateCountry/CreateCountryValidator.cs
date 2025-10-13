using FluentValidation;

namespace CleanCut.Application.Commands.Countries.CreateCountry;

/// <summary>
/// Validator for CreateCountryCommand
/// </summary>
public class CreateCountryCommandValidator : AbstractValidator<CreateCountryCommand>
{
    public CreateCountryCommandValidator()
    {
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