using Microsoft.EntityFrameworkCore;
using CleanCut.Domain.Entities;
using CleanCut.Domain.Repositories;
using CleanCut.Infrastructure.Data.Context;

namespace CleanCut.Infrastructure.Data.Repositories;

/// <summary>
/// Repository implementation for OrderLineItem entity
/// </summary>
public class OrderLineItemRepository : BaseRepository<OrderLineItem>, IOrderLineItemRepository
{
    public OrderLineItemRepository(CleanCutDbContext context) : base(context)
    {
    }

    public override async Task<OrderLineItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(oli => oli.Order)
            .Include(oli => oli.Product)
            .FirstOrDefaultAsync(oli => oli.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<OrderLineItem>> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(oli => oli.Product)
            .Where(oli => oli.OrderId == orderId)
            .OrderBy(oli => oli.ProductName)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<OrderLineItem>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(oli => oli.Order)
            .Where(oli => oli.ProductId == productId)
            .OrderByDescending(oli => oli.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    // Implement IOrderLineItemRepository methods that return specific types
    async Task<OrderLineItem> IOrderLineItemRepository.AddAsync(OrderLineItem orderLineItem, CancellationToken cancellationToken)
    {
        return await AddAsync(orderLineItem, cancellationToken);
    }

    async Task<OrderLineItem> IOrderLineItemRepository.UpdateAsync(OrderLineItem orderLineItem, CancellationToken cancellationToken)
    {
        await UpdateAsync(orderLineItem, cancellationToken);
        return orderLineItem;
    }

    async Task<bool> IOrderLineItemRepository.DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var orderLineItem = await DbSet.FindAsync([id], cancellationToken);
        if (orderLineItem == null)
            return false;

        await DeleteAsync(orderLineItem, cancellationToken);
        return true;
    }
}