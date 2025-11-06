using MediatR;
using CleanCut.Application.DTOs;
using CleanCut.Application.Common.Interfaces;

namespace CleanCut.Application.Queries.Orders.GetAllOrders;

/// <summary>
/// Query to get all orders
/// </summary>
public record GetAllOrdersQuery() : IRequest<IReadOnlyList<OrderInfo>>, ICacheableQuery
{
    public string CacheKey => "order:all";
    public TimeSpan? Expiration => TimeSpan.FromMinutes(10);
}