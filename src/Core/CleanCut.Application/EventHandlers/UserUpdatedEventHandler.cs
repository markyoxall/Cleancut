using CleanCut.Application.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CleanCut.Application.EventHandlers;

/// <summary>
/// Handles UserUpdatedNotification
/// </summary>
public class UserUpdatedEventHandler : INotificationHandler<UserUpdatedNotification>
{
    private readonly ILogger<UserUpdatedEventHandler> _logger;

    public UserUpdatedEventHandler(ILogger<UserUpdatedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(UserUpdatedNotification notification, CancellationToken cancellationToken)
    {
        var user = notification.DomainEvent.User;
        var updatedField = notification.DomainEvent.UpdatedField;
        
        _logger.LogInformation("User updated: {UserId} - Field: {UpdatedField}", 
            user.Id, 
            updatedField);

        // Here you could add additional logic like:
        // - Auditing changes
        // - Invalidating related caches
        // - Sending notifications to subscribers
        // - Triggering business workflows

        return Task.CompletedTask;
    }
}