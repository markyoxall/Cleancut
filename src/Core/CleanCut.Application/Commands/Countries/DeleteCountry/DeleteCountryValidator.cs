using FluentValidation;

namespace CleanCut.Application.Commands.Countries.DeleteCountry;

/// <summary>
/// Validator for DeleteCountryCommand
/// </summary>
public class DeleteCountryCommandValidator : AbstractValidator<DeleteCountryCommand>
{
    public DeleteCountryCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Country ID is required");
    }
}