using MediatR;
using CleanCut.Application.DTOs;
using CleanCut.Application.Common.Interfaces;

namespace CleanCut.Application.Commands.Customers.CreateCustomer;

/// <summary>
/// Command to create a new customer (formerly user)
/// </summary>
public record CreateCustomerCommand(
    string FirstName,
    string LastName,
    string Email
) : IRequest<CustomerInfo>, ICacheInvalidator
{
    // Invalidate the shared customers list and any email-based lookup cache entries
    public IEnumerable<string> CacheKeysToInvalidate =>
        new[] { "customers:all", $"customers:email:{Email.ToLowerInvariant()}" };
}
