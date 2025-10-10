using CleanCut.Application.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CleanCut.Application.EventHandlers;

/// <summary>
/// Handles UserCreatedNotification
/// </summary>
public class UserCreatedEventHandler : INotificationHandler<UserCreatedNotification>
{
    private readonly ILogger<UserCreatedEventHandler> _logger;

    public UserCreatedEventHandler(ILogger<UserCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(UserCreatedNotification notification, CancellationToken cancellationToken)
    {
        var user = notification.DomainEvent.User;
        
        _logger.LogInformation("User created: {UserId} - {UserName} ({Email})", 
            user.Id, 
            user.GetFullName(), 
            user.Email);

        // Here you could add additional logic like:
        // - Sending welcome emails
        // - Creating user profiles
        // - Initializing user settings
        // - Triggering analytics events

        return Task.CompletedTask;
    }
}