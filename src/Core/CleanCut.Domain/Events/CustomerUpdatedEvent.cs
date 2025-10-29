using CleanCut.Domain.Common;
using CleanCut.Domain.Entities;

namespace CleanCut.Domain.Events;

/// <summary>
/// Domain event raised when a customer is updated
/// </summary>
public class CustomerUpdatedEvent : DomainEvent
{
    public Customer Customer { get; }
    public string UpdatedField { get; }

    public CustomerUpdatedEvent(Customer customer, string updatedField)
    {
        Customer = customer;
   UpdatedField = updatedField;
    }
}