using CleanCut.Application.DTOs;

namespace CleanCut.BlazorWebApp.Services;

public interface IShoppingCartService
{
    Task AddItemAsync(Guid productId, int quantity, CancellationToken cancellationToken = default);
    Task RemoveItemAsync(Guid productId, CancellationToken cancellationToken = default);
    Task UpdateQuantityAsync(Guid productId, int quantity, CancellationToken cancellationToken = default);
    Task ClearAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CartItemDto>> GetItemsAsync(CancellationToken cancellationToken = default);
    Task<decimal> GetTotalAsync(CancellationToken cancellationToken = default);
}
