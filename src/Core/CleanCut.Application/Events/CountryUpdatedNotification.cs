using CleanCut.Domain.Events;
using MediatR;

namespace CleanCut.Application.Events;

public class CountryUpdatedNotification : INotification
{
    public CountryUpdatedEvent DomainEvent { get; }

    public CountryUpdatedNotification(CountryUpdatedEvent domainEvent)
    {
        DomainEvent = domainEvent;
    }
}
