using CleanCut.Application.Events;
using MediatR;
using CleanCut.Application.DTOs;
using Microsoft.Extensions.Logging;
using CleanCut.Application.Common.Interfaces;

namespace CleanCut.API.EventHandlers;

/// <summary>
/// Handles ProductCreated domain event and publishes to RabbitMQ
/// </summary>
public class ProductCreatedNotificationHandler : INotificationHandler<ProductCreatedNotification>
{
    private readonly IIntegrationEventPublisher _publisher;
    private readonly ILogger<ProductCreatedNotificationHandler> _logger;

    public ProductCreatedNotificationHandler(
        IIntegrationEventPublisher publisher,
        ILogger<ProductCreatedNotificationHandler> logger)
    {
        _publisher = publisher;
        _logger = logger;
    }

    public async Task Handle(ProductCreatedNotification notification, CancellationToken cancellationToken)
    {
        var product = notification.DomainEvent.Product;

        var dto = new ProductInfo
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            IsAvailable = product.IsAvailable
        };

        await _publisher.PublishAsync("product.created", dto, cancellationToken);
        _logger.LogInformation("✅ Published product.created for {ProductId}", product.Id);
    }
}
