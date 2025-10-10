using CleanCut.Domain.Common;
using CleanCut.Domain.Entities;

namespace CleanCut.Domain.Events;

/// <summary>
/// Domain event raised when a user is created
/// </summary>
public class UserCreatedEvent : DomainEvent
{
    public User User { get; }

    public UserCreatedEvent(User user)
    {
        User = user;
    }
}