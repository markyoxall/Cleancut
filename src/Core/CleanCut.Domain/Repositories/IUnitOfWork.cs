using CleanCut.Domain.Common;

namespace CleanCut.Domain.Repositories;

/// <summary>
/// Unit of Work pattern interface for managing transactions
/// </summary>
public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IProductRepository Products { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets all entities that have domain events
    /// </summary>
    IEnumerable<IHasDomainEvents> GetEntitiesWithDomainEvents();
}