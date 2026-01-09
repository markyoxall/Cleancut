using CleanCut.Application.Events;
using MediatR;
using CleanCut.Application.DTOs;
using Microsoft.Extensions.Logging;
using CleanCut.Application.Common.Interfaces;

namespace CleanCut.API.EventHandlers;

/// <summary>
/// Handles OrderUpdated domain event and publishes to RabbitMQ
/// </summary>
public class OrderUpdatedNotificationHandler : INotificationHandler<OrderUpdatedNotification>
{
    private readonly IIntegrationEventPublisher _publisher;
    private readonly ILogger<OrderUpdatedNotificationHandler> _logger;

    public OrderUpdatedNotificationHandler(
        IIntegrationEventPublisher publisher,
        ILogger<OrderUpdatedNotificationHandler> logger)
    {
        _publisher = publisher;
        _logger = logger;
    }

    public async Task Handle(OrderUpdatedNotification notification, CancellationToken cancellationToken)
    {
        var order = notification.DomainEvent.Order;

        var dto = new OrderInfo
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            OrderNumber = order.OrderNumber,
            OrderDate = order.OrderDate,
            Status = order.Status,
            TotalAmount = order.TotalAmount
        };

        await _publisher.PublishAsync("order.updated", dto, cancellationToken);
        _logger.LogInformation("✅ Published order.updated for {OrderId}", order.Id);
    }
}
