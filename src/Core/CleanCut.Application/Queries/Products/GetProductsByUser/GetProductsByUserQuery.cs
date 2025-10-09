using MediatR;
using CleanCut.Application.DTOs;

namespace CleanCut.Application.Queries.Products.GetProductsByUser;

/// <summary>
/// Query to get all products for a specific user
/// </summary>
public record GetProductsByUserQuery(Guid UserId) : IRequest<IReadOnlyList<ProductDto>>;