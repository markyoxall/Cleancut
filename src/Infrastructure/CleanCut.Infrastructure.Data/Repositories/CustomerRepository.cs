using Microsoft.EntityFrameworkCore;
using CleanCut.Domain.Entities;
using CleanCut.Domain.Repositories;
using CleanCut.Infrastructure.Data.Context;

namespace CleanCut.Infrastructure.Data.Repositories;

/// <summary>
/// Repository implementation for Customer entity
/// </summary>
public class CustomerRepository : BaseRepository<Customer>, ICustomerRepository
{
    public CustomerRepository(CleanCutDbContext context) : base(context)
    {
    }

    public async Task<Customer?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Email == email.ToLowerInvariant(), cancellationToken);
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .AnyAsync(x => x.Email == email.ToLowerInvariant(), cancellationToken);
    }

    public override async Task<IReadOnlyList<Customer>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .OrderBy(x => x.LastName)
            .ThenBy(x => x.FirstName)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Customer>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
    {
        var idList = ids?.Distinct().ToList() ?? new List<Guid>();
        if (!idList.Any()) return Array.Empty<Customer>();

        return await DbSet
            .AsNoTracking()
            .Where(c => idList.Contains(c.Id))
            .ToListAsync(cancellationToken);
    }
}
