using MediatR;
using Microsoft.Extensions.Logging;
using CleanCut.Domain.Events;

namespace CleanCut.Application.EventHandlers;

/// <summary>
/// Event handler for OrderUpdatedEvent
/// </summary>
public class OrderUpdatedEventHandler : INotificationHandler<OrderUpdatedEvent>
{
    private readonly ILogger<OrderUpdatedEventHandler> _logger;

    public OrderUpdatedEventHandler(ILogger<OrderUpdatedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(OrderUpdatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Order updated: {OrderId} - {OrderNumber} - Update Type: {UpdateType}",
            notification.Order.Id,
            notification.Order.OrderNumber,
            notification.UpdateType);

        // Here you could add additional logic based on update type:
        // - LineItems: Recalculate pricing, update inventory
        // - ShippingAddress: Recalculate shipping costs
        // - BillingAddress: Update payment processing
        // - Notes: Log customer communication

        return Task.CompletedTask;
    }
}