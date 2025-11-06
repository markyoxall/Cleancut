using MediatR;
using CleanCut.Application.DTOs;
using CleanCut.Application.Common.Interfaces;

namespace CleanCut.Application.Queries.Orders.GetOrder;

/// <summary>
/// Query to get a single order by ID
/// </summary>
public record GetOrderQuery(Guid Id) : IRequest<OrderInfo?>, ICacheableQuery
{
    public string CacheKey => $"order:{Id}";
    public TimeSpan? Expiration => TimeSpan.FromMinutes(5);
}