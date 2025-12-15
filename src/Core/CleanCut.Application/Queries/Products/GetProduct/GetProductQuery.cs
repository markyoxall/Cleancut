using MediatR;
using CleanCut.Application.DTOs;
using CleanCut.Application.Common.Interfaces;
using System;

namespace CleanCut.Application.Queries.Products.GetProduct;

/// <summary>
/// Query to get a product by ID
/// </summary>
public record GetProductQuery(Guid Id) : IRequest<ProductInfo?>, ICacheableQuery
{
    public string CacheKey => $"products:{Id}";
    public TimeSpan? Expiration => TimeSpan.FromHours(1);
}
