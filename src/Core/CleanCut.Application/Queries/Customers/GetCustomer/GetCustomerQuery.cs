using MediatR;
using CleanCut.Application.DTOs;
using CleanCut.Application.Common.Interfaces;

namespace CleanCut.Application.Queries.Customers.GetCustomer;

/// <summary>
/// Query to get a customer by ID
/// </summary>
public record GetCustomerQuery(Guid Id) : IRequest<CustomerInfo?>, ICacheableQuery
{
    public string CacheKey => $"customer:id:{Id}";
    public TimeSpan? Expiration => TimeSpan.FromMinutes(30);
}