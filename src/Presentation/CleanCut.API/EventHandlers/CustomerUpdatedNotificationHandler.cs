using CleanCut.Application.Common.Interfaces;
using CleanCut.Application.Events;
using CleanCut.Application.DTOs;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CleanCut.API.EventHandlers;

/// <summary>
/// Handles CustomerUpdated domain event and publishes to RabbitMQ
/// </summary>
public class CustomerUpdatedNotificationHandler : INotificationHandler<CustomerUpdatedNotification>
{
    private readonly ICacheService _cacheService;
    private readonly IIntegrationEventPublisher _publisher;
    private readonly ILogger<CustomerUpdatedNotificationHandler> _logger;

    public CustomerUpdatedNotificationHandler(
        ICacheService cacheService,
        IIntegrationEventPublisher publisher,
        ILogger<CustomerUpdatedNotificationHandler> logger)
    {
        _cacheService = cacheService;
        _publisher = publisher;
        _logger = logger;
    }

    public async Task Handle(CustomerUpdatedNotification notification, CancellationToken cancellationToken)
    {
        var customer = notification.DomainEvent.Customer;

        // Invalidate cache
        try
        {
            await _cacheService.RemoveByPatternAsync($"customers:*", cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cache invalidation failed for customer {CustomerId}", customer.Id);
        }

        // Publish to RabbitMQ
        var dto = new CustomerInfo
        {
            Id = customer.Id,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            Email = customer.Email,
            IsActive = customer.IsActive,
            CreatedAt = customer.CreatedAt,
            UpdatedAt = customer.UpdatedAt,
            FullName = customer.GetFullName()
        };

        await _publisher.PublishAsync("customer.updated", dto, cancellationToken);
        _logger.LogInformation("✅ Published customer.updated for {CustomerId}", customer.Id);
    }
}
