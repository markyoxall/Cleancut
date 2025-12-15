using CleanCut.Application.Events;
using MediatR;
using CleanCut.API.Services;
using Microsoft.AspNetCore.SignalR;
using CleanCut.API.Hubs;
using CleanCut.Application.DTOs;
using Microsoft.Extensions.Logging;

namespace CleanCut.API.EventHandlers;

public class OrderStatusChangedNotificationHandler : INotificationHandler<OrderStatusChangedNotification>
{
    private readonly IIntegrationEventProcessor _processor;
    private readonly IHubContext<NotificationsHub> _hub;
    private readonly ILogger<OrderStatusChangedNotificationHandler> _logger;

    public OrderStatusChangedNotificationHandler(IIntegrationEventProcessor processor, IHubContext<NotificationsHub> hub, ILogger<OrderStatusChangedNotificationHandler> logger)
    {
        _processor = processor;
        _hub = hub;
        _logger = logger;
    }

    public async Task Handle(OrderStatusChangedNotification notification, CancellationToken cancellationToken)
    {
        var eventObj = notification.DomainEvent;
        var order = eventObj.Order;

        var dto = new CleanCut.Application.DTOs.OrderInfo
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            OrderNumber = order.OrderNumber,
            OrderDate = order.OrderDate,
            Status = order.Status,
            TotalAmount = order.TotalAmount
        };

        await _processor.ProcessAsync("order.status.changed", dto, cancellationToken);

        try
        {
            await _hub.Clients.All.SendAsync("OrderStatusChanged", dto, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "SignalR broadcast failed for order status change {OrderId}", order.Id);
        }
    }
}
