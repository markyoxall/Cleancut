using FluentValidation;
using CleanCut.WinApp.Views.Countries;

namespace CleanCut.WinApp.Validators
{
    public class CountryEditViewModelValidator : AbstractValidator<CountryEditViewModel>
    {
        public CountryEditViewModelValidator()
        {
            RuleFor(x => x.CountryName)
                .NotEmpty().WithMessage("Country name is required")
                .MinimumLength(2).WithMessage("Country name must be at least 2 characters")
                .MaximumLength(100).WithMessage("Country name cannot exceed 100 characters");
        }
    }
}
