using CleanCut.Application.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CleanCut.Application.EventHandlers;

/// <summary>
/// Handles CustomerCreatedNotification
/// </summary>
public class CustomerCreatedEventHandler : INotificationHandler<CustomerCreatedNotification>
{
    private readonly ILogger<CustomerCreatedEventHandler> _logger;

    public CustomerCreatedEventHandler(ILogger<CustomerCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(CustomerCreatedNotification notification, CancellationToken cancellationToken)
    {
        var customer = notification.DomainEvent.Customer;
        
        _logger.LogInformation("Customer created: {CustomerId} - {CustomerName} ({Email})",
            customer.Id,
            customer.GetFullName(),
            customer.Email);

        // Additional logic like sending welcome emails, initializing settings, etc.

        return Task.CompletedTask;
    }
}