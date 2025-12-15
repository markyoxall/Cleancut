using MediatR;
using CleanCut.Application.DTOs;
using CleanCut.Application.Common.Interfaces;

namespace CleanCut.Application.Commands.Products.UpdateProduct;

/// <summary>
/// Command to update an existing product
/// </summary>
public record UpdateProductCommand(
    Guid Id,
    string Name,
    string Description,
    decimal Price
) : IRequest<ProductInfo>, ICacheInvalidator
{
    public IEnumerable<string> CacheKeysToInvalidate => new[] { "products:all", $"products:{Id}" };
}
