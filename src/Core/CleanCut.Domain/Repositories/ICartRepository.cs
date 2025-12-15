using CleanCut.Domain.Entities;

namespace CleanCut.Domain.Repositories;

public interface ICartRepository
{
    Task<Cart?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Cart?> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default);
    Task<Cart> AddAsync(Cart cart, CancellationToken cancellationToken = default);
    Task UpdateAsync(Cart cart, CancellationToken cancellationToken = default);
    Task DeleteAsync(Cart cart, CancellationToken cancellationToken = default);
}
