using FluentValidation;
using CleanCut.BlazorSPA.Pages.Models;

namespace CleanCut.BlazorSPA.Pages.Validation;

public class CustomerValidator : AbstractValidator<SimpleCustomer>
{
    public CustomerValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(50).WithMessage("First name cannot exceed 50 characters");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Please enter a valid email address");

        RuleFor(x => x.Age)
            .InclusiveBetween(0, 150).WithMessage("Age must be between 0 and 150").When(x => x.Age.HasValue);

        RuleFor(x => x.Balance)
            .GreaterThanOrEqualTo(0).WithMessage("Balance cannot be negative").When(x => x.Balance.HasValue);

        RuleFor(x => x.Website)
            .Must(uri => uri == null || uri.IsAbsoluteUri).WithMessage("Website must be a valid absolute URL");

        RuleFor(x => x.Tags)
            .Must(tags => tags == null || tags.Count <= 10).WithMessage("At most 10 tags are allowed");
    }
}
