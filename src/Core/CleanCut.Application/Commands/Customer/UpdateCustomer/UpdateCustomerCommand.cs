using MediatR;
using CleanCut.Application.DTOs;

namespace CleanCut.Application.Commands.Customers.UpdateCustomer;

/// <summary>
/// Command to update an existing customer (formerly user)
/// </summary>
public record UpdateCustomerCommand(
    Guid Id,
    string FirstName,
    string LastName,
    string Email
) : IRequest<CustomerInfo>;