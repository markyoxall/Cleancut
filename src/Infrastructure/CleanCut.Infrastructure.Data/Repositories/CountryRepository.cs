using CleanCut.Domain.Entities;
using CleanCut.Domain.Repositories;
using CleanCut.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;


namespace CleanCut.Infrastructure.Data.Repositories;

public class CountryRepository : BaseRepository<Country>, ICountryRepository
{
    public  CountryRepository(CleanCutDbContext context) : base(context)
    {
    }

    public override async Task<Country?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var country = await DbSet.FindAsync(new object[] { id }, cancellationToken);
        if (country == null)
            throw new InvalidOperationException($"Country with id '{id}' not found.");
        return country;
    }

    public override async Task<IReadOnlyList<Country>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

}
