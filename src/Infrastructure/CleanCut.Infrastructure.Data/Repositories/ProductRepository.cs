using Microsoft.EntityFrameworkCore;
using CleanCut.Domain.Entities;
using CleanCut.Domain.Repositories;
using CleanCut.Infrastructure.Data.Context;

namespace CleanCut.Infrastructure.Data.Repositories;

/// <summary>
/// Repository implementation for Product entity
/// </summary>
public class ProductRepository : BaseRepository<Product>, IProductRepository
{
    public ProductRepository(CleanCutDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Product>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(x => x.CustomerId == customerId)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public override async Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }
}