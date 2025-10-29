using CleanCut.Application.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CleanCut.Application.EventHandlers;

/// <summary>
/// Handles CustomerUpdatedNotification
/// </summary>
public class CustomerUpdatedEventHandler : INotificationHandler<CustomerUpdatedNotification>
{
    private readonly ILogger<CustomerUpdatedEventHandler> _logger;

    public CustomerUpdatedEventHandler(ILogger<CustomerUpdatedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(CustomerUpdatedNotification notification, CancellationToken cancellationToken)
    {
        var customer = notification.DomainEvent.Customer;
        var updatedField = notification.DomainEvent.UpdatedField;
        
        _logger.LogInformation("Customer updated: {CustomerId} - Field: {UpdatedField}",
            customer.Id,
            updatedField);

        // Additional logic like auditing, cache invalidation, etc.

        return Task.CompletedTask;
    }
}