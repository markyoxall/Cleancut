using CleanCut.Domain.Common;
using CleanCut.Domain.Entities;

namespace CleanCut.Domain.Events;

public class ProductUpdatedEvent : DomainEvent
{
    public Product Product { get; }
    public string UpdatedField { get; }

    public ProductUpdatedEvent(Product product, string updatedField)
    {
        Product = product;
        UpdatedField = updatedField;
    }
}
