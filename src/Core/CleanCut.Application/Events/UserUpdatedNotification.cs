using CleanCut.Domain.Events;
using MediatR;

namespace CleanCut.Application.Events;

/// <summary>
/// MediatR notification wrapper for UserUpdatedEvent
/// </summary>
public class UserUpdatedNotification : INotification
{
    public UserUpdatedEvent DomainEvent { get; }

    public UserUpdatedNotification(UserUpdatedEvent domainEvent)
    {
        DomainEvent = domainEvent;
    }
}