using MediatR;
using Microsoft.Extensions.Logging;
using CleanCut.Domain.Events;

namespace CleanCut.Application.EventHandlers;

/// <summary>
/// Event handler for OrderStatusChangedEvent
/// </summary>
public class OrderStatusChangedEventHandler : INotificationHandler<OrderStatusChangedEvent>
{
    private readonly ILogger<OrderStatusChangedEventHandler> _logger;

    public OrderStatusChangedEventHandler(ILogger<OrderStatusChangedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(OrderStatusChangedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Order status changed: {OrderId} - {OrderNumber} from {PreviousStatus} to {NewStatus}",
            notification.Order.Id,
            notification.Order.OrderNumber,
            notification.PreviousStatus,
            notification.NewStatus);

        // Here you could add additional logic based on status change:
        // - Pending -> Confirmed: Process payment, reserve inventory
        // - Confirmed -> Shipped: Generate shipping labels, notify customer
        // - Shipped -> Delivered: Update customer records, trigger feedback request
        // - Any -> Cancelled: Release inventory, process refunds

        return Task.CompletedTask;
    }
}