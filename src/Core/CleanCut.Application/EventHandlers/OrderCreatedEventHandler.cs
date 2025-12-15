using MediatR;
using Microsoft.Extensions.Logging;
using CleanCut.Application.Events;

namespace CleanCut.Application.EventHandlers;

/// <summary>
/// Event handler for OrderCreatedNotification
/// </summary>
public class OrderCreatedEventHandler : INotificationHandler<OrderCreatedNotification>
{
    private readonly ILogger<OrderCreatedEventHandler> _logger;

    public OrderCreatedEventHandler(ILogger<OrderCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(OrderCreatedNotification notification, CancellationToken cancellationToken)
    {
        var order = notification.DomainEvent.Order;
        
        _logger.LogInformation("Order created: {OrderId} - {OrderNumber} for Customer {CustomerId}",
            order.Id, 
            order.OrderNumber,
            order.CustomerId);

        // Here you could add additional logic such as:
        // - Send email notifications
        // - Update inventory
        // - Log to audit trail
        // - Trigger other business processes

        return Task.CompletedTask;
    }
}