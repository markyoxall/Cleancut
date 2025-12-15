using MediatR;
using Microsoft.Extensions.Logging;
using CleanCut.Application.Events;

namespace CleanCut.Application.EventHandlers;

/// <summary>
/// Event handler for OrderStatusChangedNotification
/// </summary>
public class OrderStatusChangedEventHandler : INotificationHandler<OrderStatusChangedNotification>
{
    private readonly ILogger<OrderStatusChangedEventHandler> _logger;

    public OrderStatusChangedEventHandler(ILogger<OrderStatusChangedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(OrderStatusChangedNotification notification, CancellationToken cancellationToken)
    {
        var order = notification.DomainEvent.Order;
        
        _logger.LogInformation("Order status changed: {OrderId} - {OrderNumber} from {PreviousStatus} to {NewStatus}",
            order.Id,
            order.OrderNumber,
            notification.DomainEvent.PreviousStatus,
            notification.DomainEvent.NewStatus);

        // Here you could add additional logic based on status change:
        // - Pending -> Confirmed: Process payment, reserve inventory
        // - Confirmed -> Shipped: Generate shipping labels, notify customer
        // - Shipped -> Delivered: Update customer records, trigger feedback request
        // - Any -> Cancelled: Release inventory, process refunds

        return Task.CompletedTask;
    }
}