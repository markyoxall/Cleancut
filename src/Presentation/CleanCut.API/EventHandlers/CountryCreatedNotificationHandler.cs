using CleanCut.Application.Events;
using MediatR;
using CleanCut.API.Services;
using Microsoft.AspNetCore.SignalR;
using CleanCut.API.Hubs;
using CleanCut.Application.DTOs;
using Microsoft.Extensions.Logging;

namespace CleanCut.API.EventHandlers;

public class CountryCreatedNotificationHandler : INotificationHandler<CountryCreatedNotification>
{
    private readonly IIntegrationEventProcessor _processor;
    private readonly IHubContext<NotificationsHub> _hub;
    private readonly ILogger<CountryCreatedNotificationHandler> _logger;

    public CountryCreatedNotificationHandler(IIntegrationEventProcessor processor, IHubContext<NotificationsHub> hub, ILogger<CountryCreatedNotificationHandler> logger)
    {
        _processor = processor;
        _hub = hub;
        _logger = logger;
    }

    public async Task Handle(CountryCreatedNotification notification, CancellationToken cancellationToken)
    {
        var country = notification.DomainEvent.Country;

        var dto = new CleanCut.Application.DTOs.CountryInfo
        {
            Id = country.Id,
            Name = country.Name,
            Code = country.Code
        };

        await _processor.ProcessAsync("country.created", dto, cancellationToken);

        try
        {
            await _hub.Clients.All.SendAsync("CountryCreated", dto, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "SignalR broadcast failed for country {CountryId}", country.Id);
        }
    }
}
