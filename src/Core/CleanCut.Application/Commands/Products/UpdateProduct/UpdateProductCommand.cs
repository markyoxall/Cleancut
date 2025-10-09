using MediatR;
using CleanCut.Application.DTOs;

namespace CleanCut.Application.Commands.Products.UpdateProduct;

/// <summary>
/// Command to update an existing product
/// </summary>
public record UpdateProductCommand(
    Guid Id,
    string Name,
    string Description,
    decimal Price
) : IRequest<ProductDto>;