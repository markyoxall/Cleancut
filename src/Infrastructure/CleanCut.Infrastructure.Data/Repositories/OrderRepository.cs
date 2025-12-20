using Microsoft.EntityFrameworkCore;
using CleanCut.Domain.Entities;
using CleanCut.Domain.Repositories;
using CleanCut.Infrastructure.Data.Context;

namespace CleanCut.Infrastructure.Data.Repositories;

/// <summary>
/// Repository implementation for Order entity
/// </summary>
public class OrderRepository : BaseRepository<Order>, IOrderRepository
{
    public OrderRepository(CleanCutDbContext context) : base(context)
    {
    }

    public override async Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(o => o.OrderLineItems)
            .Include(o => o.Customer)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task<Order?> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(o => o.OrderLineItems)
            .Include(o => o.Customer)
            .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber, cancellationToken);
    }

    public override async Task<IReadOnlyList<Order>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(o => o.OrderLineItems)
            .Include(o => o.Customer)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Order>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(o => o.OrderLineItems)
            .Include(o => o.Customer)
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Order>> GetByStatusAsync(OrderStatus status, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(o => o.OrderLineItems)
            .Include(o => o.Customer)
            .Where(o => o.Status == status)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Order>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(o => o.OrderLineItems)
            .Include(o => o.Customer)
            .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> OrderNumberExistsAsync(string orderNumber, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AnyAsync(o => o.OrderNumber == orderNumber, cancellationToken);
    }

    // Implement IOrderRepository methods that return Task<Order> and Task<bool>
    async Task<Order> IOrderRepository.AddAsync(Order order, CancellationToken cancellationToken)
    {
        return await AddAsync(order, cancellationToken);
    }

    async Task<Order> IOrderRepository.UpdateAsync(Order order, CancellationToken cancellationToken)
    {
        await UpdateAsync(order, cancellationToken);
        return order;
    }

    async Task<bool> IOrderRepository.DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        // Use FindAsync with key values to locate the entity
        var order = await DbSet.FindAsync(new object[] { id }, cancellationToken);
        if (order == null)
            return false;

        await DeleteAsync(order, cancellationToken);
        return true;
    }
}
