using MediatR;
using CleanCut.Application.DTOs;

namespace CleanCut.Application.Queries.Products.GetProduct;

/// <summary>
/// Query to get a product by ID
/// </summary>
public record GetProductQuery(Guid Id) : IRequest<ProductDto?>;