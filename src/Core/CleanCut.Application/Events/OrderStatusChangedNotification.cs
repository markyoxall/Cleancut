using CleanCut.Domain.Events;
using MediatR;

namespace CleanCut.Application.Events;

/// <summary>
/// MediatR notification wrapper for OrderStatusChangedEvent
/// </summary>
public class OrderStatusChangedNotification : INotification
{
    public OrderStatusChangedEvent DomainEvent { get; }

    public OrderStatusChangedNotification(OrderStatusChangedEvent domainEvent)
    {
        DomainEvent = domainEvent;
    }
}