using FluentValidation;
using CleanCut.BlazorSPA.Pages.Models;

namespace CleanCut.BlazorSPA.Pages.Validation;

public class CountryValidator : AbstractValidator<SimpleCountry>
{
    public CountryValidator()
    {
        RuleFor(x => x.CountryName)
            .NotEmpty().WithMessage("Country name is required")
            .MinimumLength(2).WithMessage("Country name must be at least 2 characters")
            .MaximumLength(100).WithMessage("Country name cannot exceed 100 characters");
    }
}
