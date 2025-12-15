using CleanCut.Application.Common.Interfaces;
using CleanCut.Application.Common.Models;
using CleanCut.Infrastructure.Data.Context;
using CleanCut.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace CleanCut.Infrastructure.Data.Repositories;

public class IdempotencyRepository : IIdempotencyRepository
{
    private readonly CleanCutDbContext _context;

    public IdempotencyRepository(CleanCutDbContext context)
    {
        _context = context;
    }

    public async Task<IdempotencyEntry?> GetByKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Set<IdempotencyRecord>().AsNoTracking().FirstOrDefaultAsync(x => x.Key == key, cancellationToken);
        if (entity == null) return null;

        return new IdempotencyEntry
        {
            Key = entity.Key,
            CreatedAt = entity.CreatedAt,
            UserId = entity.UserId,
            RequestHash = entity.RequestHash,
            ResponsePayload = entity.ResponsePayload,
            ResponseStatus = entity.ResponseStatus,
            ResponseHeaders = entity.ResponseHeaders
        };
    }

    public async Task AddAsync(IdempotencyEntry record, CancellationToken cancellationToken = default)
    {
        var entity = new IdempotencyRecord
        {
            Key = record.Key,
            CreatedAt = record.CreatedAt == default ? DateTime.UtcNow : record.CreatedAt,
            UserId = record.UserId,
            RequestHash = record.RequestHash,
            ResponsePayload = record.ResponsePayload,
            ResponseStatus = record.ResponseStatus,
            ResponseHeaders = record.ResponseHeaders
        };

        _context.Set<IdempotencyRecord>().Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(IdempotencyEntry record, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Set<IdempotencyRecord>().FirstOrDefaultAsync(x => x.Key == record.Key, cancellationToken);
        if (entity == null) return;

        entity.ResponsePayload = record.ResponsePayload;
        entity.ResponseStatus = record.ResponseStatus;
        entity.ResponseHeaders = record.ResponseHeaders;

        _context.Set<IdempotencyRecord>().Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
