using CleanCut.Application.Common.Interfaces;
using CleanCut.Application.Events;
using CleanCut.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using CleanCut.API.Hubs;
using Microsoft.Extensions.Logging;

namespace CleanCut.API.EventHandlers;

/// <summary>
/// Centralized integration handler that reacts to domain event notifications and performs:
///  - cache invalidation
///  - integration publish (RabbitMQ)
///  - SignalR broadcast to connected clients
/// </summary>
public class IntegrationEventNotificationHandler : INotificationHandler<CustomerUpdatedNotification>
{
    private readonly ICacheService _cacheService;
    private readonly IIntegrationEventPublisher _publisher;
    private readonly IHubContext<NotificationsHub> _hubContext;
    private readonly ILogger<IntegrationEventNotificationHandler> _logger;

    public IntegrationEventNotificationHandler(
        ICacheService cacheService,
        IIntegrationEventPublisher publisher,
        IHubContext<NotificationsHub> hubContext,
        ILogger<IntegrationEventNotificationHandler> logger)
    {
        _cacheService = cacheService;
        _publisher = publisher;
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task Handle(CustomerUpdatedNotification notification, CancellationToken cancellationToken)
    {
        var customer = notification.DomainEvent.Customer;

        try
        {
            // Invalidate related cache entries (pattern-based)
            try
            {
                await _cacheService.RemoveByPatternAsync($"customers:*", cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Cache invalidation failed for customer {CustomerId}", customer.Id);
            }

            // Publish integration event for other systems
            try
            {
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
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to publish integration event for customer {CustomerId}", customer.Id);
            }

            // Broadcast to SignalR clients (best-effort)
            try
            {
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

                await _hubContext.Clients.All.SendAsync("CustomerUpdated", dto, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "SignalR broadcast failed for customer {CustomerId}", customer.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in IntegrationEventNotificationHandler for customer {CustomerId}", customer.Id);
        }
    }
}
