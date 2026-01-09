using CleanCut.Application.Events;
using MediatR;
using CleanCut.Application.DTOs;
using Microsoft.Extensions.Logging;
using CleanCut.Application.Common.Interfaces;

namespace CleanCut.API.EventHandlers;

public class CountryCreatedNotificationHandler : INotificationHandler<CountryCreatedNotification>
{
    private readonly IIntegrationEventPublisher _publisher;
    private readonly ILogger<CountryCreatedNotificationHandler> _logger;

    public CountryCreatedNotificationHandler(
        IIntegrationEventPublisher publisher,
        ILogger<CountryCreatedNotificationHandler> logger)
    {
        _publisher = publisher;
        _logger = logger;
    }

    public async Task Handle(CountryCreatedNotification notification, CancellationToken cancellationToken)
    {
        var country = notification.DomainEvent.Country;

        var dto = new CountryInfo
        {
            Id = country.Id,
            Name = country.Name,
            Code = country.Code
        };

        await _publisher.PublishAsync("country.created", dto, cancellationToken);
        _logger.LogInformation("Published country.created for {CountryId}", country.Id);
    }
}
