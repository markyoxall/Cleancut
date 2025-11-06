using MediatR;
using Microsoft.Extensions.Logging;
using CleanCut.Application.Events;

namespace CleanCut.Application.EventHandlers;

/// <summary>
/// Event handler for OrderUpdatedNotification
/// </summary>
public class OrderUpdatedEventHandler : INotificationHandler<OrderUpdatedNotification>
{
    private readonly ILogger<OrderUpdatedEventHandler> _logger;

    public OrderUpdatedEventHandler(ILogger<OrderUpdatedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(OrderUpdatedNotification notification, CancellationToken cancellationToken)
    {
        var order = notification.DomainEvent.Order;
        
        _logger.LogInformation("Order updated: {OrderId} - {OrderNumber} - Update Type: {UpdateType}",
            order.Id,
            order.OrderNumber,
            notification.DomainEvent.UpdateType);

        // Here you could add additional logic based on update type:
        // - LineItems: Recalculate pricing, update inventory
        // - ShippingAddress: Recalculate shipping costs
        // - BillingAddress: Update payment processing
        // - Notes: Log customer communication

        return Task.CompletedTask;
    }
}