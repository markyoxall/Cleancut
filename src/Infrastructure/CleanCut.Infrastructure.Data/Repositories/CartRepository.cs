using Microsoft.EntityFrameworkCore;
using CleanCut.Domain.Entities;
using CleanCut.Domain.Repositories;
using CleanCut.Infrastructure.Data.Context;

namespace CleanCut.Infrastructure.Data.Repositories;

public class CartRepository : ICartRepository
{
    private readonly CleanCutDbContext _context;

    public CartRepository(CleanCutDbContext context)
    {
        _context = context;
    }

    public async Task<Cart?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Carts.Include(c => c.Items).FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<Cart?> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default)
    {
        return await _context.Carts.Include(c => c.Items).FirstOrDefaultAsync(c => c.OwnerId == ownerId, cancellationToken);
    }

    public async Task<Cart> AddAsync(Cart cart, CancellationToken cancellationToken = default)
    {
        _context.Carts.Add(cart);
        await _context.SaveChangesAsync(cancellationToken);
        return cart;
    }

    public async Task UpdateAsync(Cart cart, CancellationToken cancellationToken = default)
    {
        _context.Carts.Update(cart);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Cart cart, CancellationToken cancellationToken = default)
    {
        _context.Carts.Remove(cart);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
