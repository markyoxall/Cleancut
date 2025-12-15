using CleanCut.Domain.Common;
using CleanCut.Domain.Entities;

namespace CleanCut.Domain.Events;

public class CountryUpdatedEvent : DomainEvent
{
    public Country Country { get; }
    public string UpdatedField { get; }

    public CountryUpdatedEvent(Country country, string updatedField)
    {
        Country = country;
        UpdatedField = updatedField;
    }
}
