using MediatR;
using CleanCut.Application.DTOs;

namespace CleanCut.Application.Queries.Customers.GetAllCustomers;

/// <summary>
/// Query to get all customers
/// </summary>
public record GetAllCustomersQuery() : IRequest<IReadOnlyList<CustomerInfo>>;