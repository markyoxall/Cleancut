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
) : IRequest<CustomerInfo>, CleanCut.Application.Common.Interfaces.ICacheInvalidator
{
    // When a customer is updated, invalidate the customers list and the individual customer cache
    public IEnumerable<string> CacheKeysToInvalidate => new[]
    {
        "customers:all",
        $"customer:id:{Id}",
        $"customers:email:{Email.ToLowerInvariant()}"
    };
}
