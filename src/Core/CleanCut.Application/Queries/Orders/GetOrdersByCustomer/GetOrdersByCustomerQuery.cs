using MediatR;
using CleanCut.Application.DTOs;

namespace CleanCut.Application.Queries.Orders.GetOrdersByCustomer;

/// <summary>
/// Query to get orders by customer ID
/// </summary>
public record GetOrdersByCustomerQuery(Guid CustomerId) : IRequest<IReadOnlyList<OrderInfo>>;