using MediatR;
using CleanCut.Application.DTOs;

namespace CleanCut.Application.Queries.Products.GetProductsByCustomer;

/// <summary>
/// Query to get all products for a specific customer
/// </summary>
public record GetProductsByCustomerQuery(Guid CustomerId) : IRequest<IReadOnlyList<ProductInfo>>;