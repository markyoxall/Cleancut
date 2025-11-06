using CleanCut.Domain.Common;
using CleanCut.Domain.Entities;

namespace CleanCut.Domain.Events;

/// <summary>
/// Domain event raised when an order is updated
/// </summary>
public class OrderUpdatedEvent : DomainEvent
{
    public Order Order { get; }
    public string UpdateType { get; }

    public OrderUpdatedEvent(Order order, string updateType)
    {
        Order = order;
        UpdateType = updateType;
    }
}