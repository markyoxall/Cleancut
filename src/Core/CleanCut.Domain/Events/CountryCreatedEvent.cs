using CleanCut.Domain.Common;
using CleanCut.Domain.Entities;

namespace CleanCut.Domain.Events;

public class CountryCreatedEvent : DomainEvent
{
    public Country Country { get; }

    public CountryCreatedEvent(Country country)
    {
        Country = country;
    }
}
