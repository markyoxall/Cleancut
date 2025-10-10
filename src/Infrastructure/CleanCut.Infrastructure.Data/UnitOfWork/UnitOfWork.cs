using Microsoft.EntityFrameworkCore.Storage;
using CleanCut.Domain.Repositories;
using CleanCut.Infrastructure.Data.Context;
using CleanCut.Infrastructure.Data.Repositories;
using CleanCut.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace CleanCut.Infrastructure.Data.UnitOfWork;

/// <summary>
/// Unit of Work implementation using Entity Framework Core
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly CleanCutDbContext _context;
    private IDbContextTransaction? _transaction;
    private bool _disposed;

    // Lazy initialization of repositories
    private IUserRepository? _users;
    private IProductRepository? _products;

    public UnitOfWork(CleanCutDbContext context)
    {
        _context = context;
    }

    public IUserRepository Users => _users ??= new UserRepository(_context);
    public IProductRepository Products => _products ??= new ProductRepository(_context);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            throw new InvalidOperationException("A transaction is already in progress");
        }

        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null)
        {
            throw new InvalidOperationException("No transaction is in progress");
        }

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            await _transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await _transaction.RollbackAsync(cancellationToken);
            throw;
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null)
        {
            throw new InvalidOperationException("No transaction is in progress");
        }

        try
        {
            await _transaction.RollbackAsync(cancellationToken);
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public IEnumerable<IHasDomainEvents> GetEntitiesWithDomainEvents()
    {
        return _context.ChangeTracker
            .Entries<IHasDomainEvents>()
            .Where(entry => entry.Entity.DomainEvents.Any())
            .Select(entry => entry.Entity);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _transaction?.Dispose();
            _context.Dispose();
            _disposed = true;
        }
    }
}