using FluentValidation;
using CleanCut.WinApp.Views.Products;

namespace CleanCut.WinApp.Validators
{
    public class ProductEditViewModelValidator : AbstractValidator<ProductEditViewModel>
    {
        public ProductEditViewModelValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Product name is required")
                .MaximumLength(100).WithMessage("Product name cannot exceed 100 characters");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required")
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");

            RuleFor(x => x.Price)
                .GreaterThanOrEqualTo(0).WithMessage("Price must be a positive value");

            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("Please select a customer");
        }
    }
}
