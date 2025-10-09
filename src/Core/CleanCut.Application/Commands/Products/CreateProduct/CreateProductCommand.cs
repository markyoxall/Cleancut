using MediatR;
using CleanCut.Application.DTOs;

namespace CleanCut.Application.Commands.Products.CreateProduct;

/// <summary>
/// Command to create a new product
/// </summary>
public record CreateProductCommand(
    string Name,
    string Description,
    decimal Price,
    Guid UserId
) : IRequest<ProductDto>;