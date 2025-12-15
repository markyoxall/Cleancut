using CleanCut.Domain.Common;
using CleanCut.Domain.Entities;

namespace CleanCut.Domain.Events;

/// <summary>
/// Domain event raised when a customer is created
/// </summary>
public class CustomerCreatedEvent : DomainEvent
{
    public Customer Customer { get; }

    public CustomerCreatedEvent(Customer customer)
    {
        Customer = customer;
  }
}