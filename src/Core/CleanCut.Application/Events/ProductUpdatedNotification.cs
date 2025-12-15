using CleanCut.Domain.Events;
using MediatR;

namespace CleanCut.Application.Events;

public class ProductUpdatedNotification : INotification
{
    public ProductUpdatedEvent DomainEvent { get; }

    public ProductUpdatedNotification(ProductUpdatedEvent domainEvent)
    {
        DomainEvent = domainEvent;
    }
}
