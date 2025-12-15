using CleanCut.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanCut.Domain.Repositories;

public interface ICountryRepository
{
    Task<Country> AddAsync(Country country, CancellationToken cancellationToken = default);
    Task DeleteAsync(Country country, CancellationToken cancellationToken = default);
 
    Task<IReadOnlyList<Country>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Country?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task UpdateAsync(Country country, CancellationToken cancellationToken = default);
}
