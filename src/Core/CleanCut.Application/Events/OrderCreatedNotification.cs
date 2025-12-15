using CleanCut.Domain.Events;
using MediatR;

namespace CleanCut.Application.Events;

/// <summary>
/// MediatR notification wrapper for OrderCreatedEvent
/// </summary>
public class OrderCreatedNotification : INotification
{
    public OrderCreatedEvent DomainEvent { get; }

    public OrderCreatedNotification(OrderCreatedEvent domainEvent)
    {
        DomainEvent = domainEvent;
    }
}