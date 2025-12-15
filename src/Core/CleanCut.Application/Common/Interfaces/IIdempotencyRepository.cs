using System.Threading;
using CleanCut.Application.Common.Models;

namespace CleanCut.Application.Common.Interfaces;

public interface IIdempotencyRepository
{
    Task<IdempotencyEntry?> GetByKeyAsync(string key, CancellationToken cancellationToken = default);
    Task AddAsync(IdempotencyEntry record, CancellationToken cancellationToken = default);
    Task UpdateAsync(IdempotencyEntry record, CancellationToken cancellationToken = default);
}
