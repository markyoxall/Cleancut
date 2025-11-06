using CleanCut.Domain.Events;
using MediatR;

namespace CleanCut.Application.Events;

/// <summary>
/// MediatR notification wrapper for OrderUpdatedEvent
/// </summary>
public class OrderUpdatedNotification : INotification
{
    public OrderUpdatedEvent DomainEvent { get; }

    public OrderUpdatedNotification(OrderUpdatedEvent domainEvent)
    {
        DomainEvent = domainEvent;
    }
}