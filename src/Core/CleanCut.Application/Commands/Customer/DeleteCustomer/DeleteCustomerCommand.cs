using MediatR;

namespace CleanCut.Application.Commands.Customers.DeleteCustomer;

/// <summary>
/// Command to delete a customer
/// </summary>
public record DeleteCustomerCommand(Guid Id) : IRequest<bool>, CleanCut.Application.Common.Interfaces.ICacheInvalidator
{
    public IEnumerable<string> CacheKeysToInvalidate => new[] { "customers:all", $"customer:id:{Id}" };
}
