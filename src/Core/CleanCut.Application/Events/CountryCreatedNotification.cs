using CleanCut.Domain.Events;
using MediatR;

namespace CleanCut.Application.Events;

public class CountryCreatedNotification : INotification
{
    public CountryCreatedEvent DomainEvent { get; }

    public CountryCreatedNotification(CountryCreatedEvent domainEvent)
    {
        DomainEvent = domainEvent;
    }
}
