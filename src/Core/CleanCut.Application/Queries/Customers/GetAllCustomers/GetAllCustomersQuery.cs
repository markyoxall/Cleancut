using MediatR;
using CleanCut.Application.DTOs;
using CleanCut.Application.Common.Interfaces;
using System;

namespace CleanCut.Application.Queries.Customers.GetAllCustomers;

/// <summary>
/// Query to get all customers
/// </summary>
public record GetAllCustomersQuery() : IRequest<IReadOnlyList<CustomerInfo>>, ICacheableQuery
{
    public string CacheKey => "customers:all";
    public TimeSpan? Expiration => TimeSpan.FromMinutes(10);
}
