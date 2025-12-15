using CleanCut.Domain.Common;
using CleanCut.Domain.Entities;

namespace CleanCut.Domain.Events;

/// <summary>
/// Domain event raised when order status changes
/// </summary>
public class OrderStatusChangedEvent : DomainEvent
{
    public Order Order { get; }
    public OrderStatus PreviousStatus { get; }
    public OrderStatus NewStatus { get; }

    public OrderStatusChangedEvent(Order order, OrderStatus previousStatus, OrderStatus newStatus)
    {
        Order = order;
        PreviousStatus = previousStatus;
        NewStatus = newStatus;
    }
}