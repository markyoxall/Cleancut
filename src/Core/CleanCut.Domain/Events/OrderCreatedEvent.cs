using CleanCut.Domain.Common;
using CleanCut.Domain.Entities;

namespace CleanCut.Domain.Events;

/// <summary>
/// Domain event raised when an order is created
/// </summary>
public class OrderCreatedEvent : DomainEvent
{
    public Order Order { get; }

    public OrderCreatedEvent(Order order)
    {
        Order = order;
    }
}