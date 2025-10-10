using CleanCut.Domain.Events;
using MediatR;

namespace CleanCut.Application.Events;

/// <summary>
/// MediatR notification wrapper for UserCreatedEvent
/// </summary>
public class UserCreatedNotification : INotification
{
    public UserCreatedEvent DomainEvent { get; }

    public UserCreatedNotification(UserCreatedEvent domainEvent)
    {
        DomainEvent = domainEvent;
    }
}