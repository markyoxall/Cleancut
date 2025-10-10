using CleanCut.Domain.Common;
using CleanCut.Domain.Entities;

namespace CleanCut.Domain.Events;

/// <summary>
/// Domain event raised when a user is updated
/// </summary>
public class UserUpdatedEvent : DomainEvent
{
    public User User { get; }
    public string UpdatedField { get; }

    public UserUpdatedEvent(User user, string updatedField)
    {
        User = user;
        UpdatedField = updatedField;
    }
}