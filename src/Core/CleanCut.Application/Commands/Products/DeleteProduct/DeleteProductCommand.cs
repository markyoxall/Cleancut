using MediatR;
using CleanCut.Application.Common.Interfaces;

namespace CleanCut.Application.Commands.Products.DeleteProduct;

/// <summary>
/// Command to delete a product
/// </summary>
public record DeleteProductCommand(Guid Id) : IRequest<bool>, ICacheInvalidator
{
    public IEnumerable<string> CacheKeysToInvalidate => new[] { "products:all", $"products:{Id}" };
}
