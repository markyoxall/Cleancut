using CleanCut.Domain.Events;
using MediatR;

namespace CleanCut.Application.Events;

/// <summary>
/// MediatR notification wrapper for UserCreatedEvent
/// </summary>
public class CustomerCreatedNotification : INotification
{
    public CustomerCreatedEvent DomainEvent { get; }

    public CustomerCreatedNotification(CustomerCreatedEvent domainEvent)
    {
        DomainEvent = domainEvent;
    }
}