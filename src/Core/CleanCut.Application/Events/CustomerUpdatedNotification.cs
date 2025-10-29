using CleanCut.Domain.Events;
using MediatR;

namespace CleanCut.Application.Events;

/// <summary>
/// MediatR notification wrapper for CustomerUpdatedEvent
/// </summary>
public class CustomerUpdatedNotification : INotification
{
    public CustomerUpdatedEvent DomainEvent { get; }

    public CustomerUpdatedNotification(CustomerUpdatedEvent domainEvent)
    {
        DomainEvent = domainEvent;
    }
}