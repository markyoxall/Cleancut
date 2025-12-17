using FluentValidation;
using CleanCut.BlazorSPA.Pages.Models;

namespace CleanCut.BlazorSPA.Pages.Validation;

public class ProductValidator : AbstractValidator<SimpleProduct>
{
    public ProductValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(200).WithMessage("Name cannot exceed 200 characters");

        RuleFor(x => x.Sku)
            .NotEmpty().WithMessage("SKU is required")
            .MaximumLength(50).WithMessage("SKU cannot exceed 50 characters");

        RuleFor(x => x.Price)
            .Must(p => p == null || p.Amount >= 0).WithMessage("Price must be a non-negative value");

        RuleFor(x => x.Price)
            .Must(p => p == null || !string.IsNullOrWhiteSpace(p.Currency)).WithMessage("Price currency is required when price is set");

        RuleFor(x => x.QuantityInStock)
            .GreaterThanOrEqualTo(0).WithMessage("Quantity must be 0 or greater").When(x => x.QuantityInStock.HasValue);

        RuleFor(x => x.DiscontinuedDate)
            .GreaterThanOrEqualTo(x => x.ReleaseDate).WithMessage("Discontinued date must be the same or after the release date")
            .When(x => x.DiscontinuedDate.HasValue && x.ReleaseDate.HasValue);

        RuleFor(x => x.Tags)
            .Must(tags => tags == null || tags.Count <= 10).WithMessage("At most 10 tags are allowed");

        RuleFor(x => x.ImageFileName)
            .MaximumLength(260).WithMessage("Image file name cannot exceed 260 characters");
    }
}
