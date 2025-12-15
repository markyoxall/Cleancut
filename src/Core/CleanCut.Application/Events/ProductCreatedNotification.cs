using CleanCut.Domain.Events;
using MediatR;

namespace CleanCut.Application.Events;

public class ProductCreatedNotification : INotification
{
    public ProductCreatedEvent DomainEvent { get; }

    public ProductCreatedNotification(ProductCreatedEvent domainEvent)
    {
        DomainEvent = domainEvent;
    }
}
