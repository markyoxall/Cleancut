using CleanCut.Application.Events;
using MediatR;
using CleanCut.API.Services;
using Microsoft.AspNetCore.SignalR;
using CleanCut.API.Hubs;
using CleanCut.Application.DTOs;
using Microsoft.Extensions.Logging;

namespace CleanCut.API.EventHandlers;

public class ProductUpdatedNotificationHandler : INotificationHandler<ProductUpdatedNotification>
{
    private readonly IIntegrationEventProcessor _processor;
    private readonly IHubContext<NotificationsHub> _hub;
    private readonly ILogger<ProductUpdatedNotificationHandler> _logger;

    public ProductUpdatedNotificationHandler(IIntegrationEventProcessor processor, IHubContext<NotificationsHub> hub, ILogger<ProductUpdatedNotificationHandler> logger)
    {
        _processor = processor;
        _hub = hub;
        _logger = logger;
    }

    public async Task Handle(ProductUpdatedNotification notification, CancellationToken cancellationToken)
    {
        var product = notification.DomainEvent.Product;

        var dto = new CleanCut.Application.DTOs.ProductInfo
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            IsAvailable = product.IsAvailable
        };

        await _processor.ProcessAsync("product.updated", dto, cancellationToken);

        try
        {
            await _hub.Clients.All.SendAsync("ProductUpdated", dto, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "SignalR broadcast failed for product {ProductId}", product.Id);
        }
    }
}
