using MediatR;
using CleanCut.Application.DTOs;
using CleanCut.Application.Common.Interfaces;
using System;

namespace CleanCut.Application.Queries.Products.GetProductsByCustomer;

/// <summary>
/// Query to get all products for a specific customer
/// </summary>
public record GetProductsByCustomerQuery(Guid CustomerId) : IRequest<IReadOnlyList<ProductInfo>>, ICacheableQuery
{
    public string CacheKey => $"products:customer:{CustomerId}";
    public TimeSpan? Expiration => TimeSpan.FromMinutes(30);
}
