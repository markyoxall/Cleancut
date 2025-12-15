using CleanCut.Application.Events;
using MediatR;
using CleanCut.API.Services;
using Microsoft.AspNetCore.SignalR;
using CleanCut.API.Hubs;
using CleanCut.Application.DTOs;
using Microsoft.Extensions.Logging;

namespace CleanCut.API.EventHandlers;

public class OrderCreatedNotificationHandler : INotificationHandler<OrderCreatedNotification>
{
    private readonly IIntegrationEventProcessor _processor;
    private readonly IHubContext<NotificationsHub> _hub;
    private readonly ILogger<OrderCreatedNotificationHandler> _logger;

    public OrderCreatedNotificationHandler(IIntegrationEventProcessor processor, IHubContext<NotificationsHub> hub, ILogger<OrderCreatedNotificationHandler> logger)
    {
        _processor = processor;
        _hub = hub;
        _logger = logger;
    }

    public async Task Handle(OrderCreatedNotification notification, CancellationToken cancellationToken)
    {
        var order = notification.DomainEvent.Order;

        var dto = new CleanCut.Application.DTOs.OrderInfo
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            OrderNumber = order.OrderNumber,
            OrderDate = order.OrderDate,
            Status = order.Status,
            TotalAmount = order.TotalAmount
        };

        await _processor.ProcessAsync("order.created", dto, cancellationToken);

        try
        {
            await _hub.Clients.All.SendAsync("OrderCreated", dto, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "SignalR broadcast failed for order {OrderId}", order.Id);
        }
    }
}
