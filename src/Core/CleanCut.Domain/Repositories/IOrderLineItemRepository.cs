using CleanCut.Domain.Entities;

namespace CleanCut.Domain.Repositories;

/// <summary>
/// Repository interface for OrderLineItem entity
/// </summary>
public interface IOrderLineItemRepository
{
    Task<OrderLineItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OrderLineItem>> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OrderLineItem>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<OrderLineItem> AddAsync(OrderLineItem orderLineItem, CancellationToken cancellationToken = default);
    Task<OrderLineItem> UpdateAsync(OrderLineItem orderLineItem, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}