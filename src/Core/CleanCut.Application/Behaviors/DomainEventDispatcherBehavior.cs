using CleanCut.Domain.Common;
using CleanCut.Domain.Events;
using CleanCut.Domain.Repositories;
using CleanCut.Application.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CleanCut.Application.Behaviors;

/// <summary>
/// Domain event dispatcher behavior for MediatR pipeline
/// </summary>
public class DomainEventDispatcherBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPublisher _publisher;
    private readonly ILogger<DomainEventDispatcherBehavior<TRequest, TResponse>> _logger;

    public DomainEventDispatcherBehavior(
        IUnitOfWork unitOfWork,
        IPublisher publisher,
        ILogger<DomainEventDispatcherBehavior<TRequest, TResponse>> logger)
    {
        _unitOfWork = unitOfWork;
        _publisher = publisher;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var response = await next();

        // Collect domain events from all entities
        var entitiesWithEvents = _unitOfWork.GetEntitiesWithDomainEvents();
        var domainEvents = entitiesWithEvents
            .SelectMany(entity => entity.DomainEvents)
            .ToList();

        // Clear domain events from entities
        foreach (var entity in entitiesWithEvents)
        {
            entity.ClearDomainEvents();
        }

        // Convert domain events to MediatR notifications and publish
        foreach (var domainEvent in domainEvents)
        {
            try
            {
                var notification = ConvertToNotification(domainEvent);
                if (notification != null)
                {
                    await _publisher.Publish(notification, cancellationToken);
                    _logger.LogInformation("Domain event published: {EventType}", domainEvent.GetType().Name);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing domain event: {EventType}", domainEvent.GetType().Name);
                // Don't rethrow - domain events shouldn't break the main flow
            }
        }

        return response;
    }

    private INotification? ConvertToNotification(DomainEvent domainEvent)
    {
        return domainEvent switch
        {
            CustomerCreatedEvent created => new CustomerCreatedNotification(created),
            CustomerUpdatedEvent updated => new CustomerUpdatedNotification(updated),
            OrderCreatedEvent oc => new OrderCreatedNotification(oc),
            OrderUpdatedEvent ou => new OrderUpdatedNotification(ou),
            OrderStatusChangedEvent os => new OrderStatusChangedNotification(os),
            ProductCreatedEvent pc => new CleanCut.Application.Events.ProductCreatedNotification(pc),
            ProductUpdatedEvent pu => new CleanCut.Application.Events.ProductUpdatedNotification(pu),
            CountryCreatedEvent cc => new CleanCut.Application.Events.CountryCreatedNotification(cc),
            CountryUpdatedEvent cu => new CleanCut.Application.Events.CountryUpdatedNotification(cu),
            _ => null
        };
    }
}
