using FluentValidation;
using CleanCut.WinApp.Views.Customers;

namespace CleanCut.WinApp.Validators
{
    public class CustomerEditViewModelValidator : AbstractValidator<CustomerEditViewModel>
    {
        public CustomerEditViewModelValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required")
                .MaximumLength(50).WithMessage("First name cannot exceed 50 characters");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required")
                .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Please enter a valid email address")
                .MaximumLength(100).WithMessage("Email cannot exceed 100 characters");
        }
    }
}
