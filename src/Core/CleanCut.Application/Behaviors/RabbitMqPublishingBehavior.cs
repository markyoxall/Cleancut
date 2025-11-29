using CleanCut.Application.Common.Interfaces;
using CleanCut.Application.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CleanCut.Application.Behaviors;

/// <summary>
/// Behavior that publishes OrderCreated events to RabbitMQ after an order is created.
/// It listens for OrderCreatedNotification published by DomainEventDispatcherBehavior.
/// </summary>
public class RabbitMqPublishingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IRabbitMqPublisher _publisher;
    private readonly ILogger<RabbitMqPublishingBehavior<TRequest, TResponse>> _logger;

    public RabbitMqPublishingBehavior(IRabbitMqPublisher publisher, ILogger<RabbitMqPublishingBehavior<TRequest, TResponse>> logger)
    {
        _publisher = publisher;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var response = await next();

        // Only act when OrderCreatedNotification was published in the pipeline - that is handled by DomainEventDispatcherBehavior
        // The DomainEventDispatcherBehavior publishes MediatR notifications; we don't have direct access here to the published events.
        // Instead, handlers that create orders will return an OrderInfo DTO which we can publish.

        try
        {
            // If the response is an OrderInfo, publish to RabbitMQ
            if (response is CleanCut.Application.DTOs.OrderInfo orderInfo)
            {
                await _publisher.PublishOrderCreatedAsync(orderInfo, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to publish order-created message for request {RequestType}", typeof(TRequest).Name);
        }

        return response;
    }
}
