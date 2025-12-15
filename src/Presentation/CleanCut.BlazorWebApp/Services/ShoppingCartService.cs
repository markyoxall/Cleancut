using CleanCut.Application.DTOs;
using System.Collections.Concurrent;

namespace CleanCut.BlazorWebApp.Services;

public class ShoppingCartService : IShoppingCartService
{
    // Simple in-memory per-server cart store keyed by user session id (for demo purposes)
    private readonly IProductApiService _productApi;
    private readonly IHttpContextAccessor? _httpContextAccessor;
    private static readonly ConcurrentDictionary<string, List<CartItemDto>> _store = new();

    public ShoppingCartService(IProductApiService productApi, IHttpContextAccessor? httpContextAccessor = null)
    {
        _productApi = productApi;
        _httpContextAccessor = httpContextAccessor;
    }

    private string GetCartKey()
    {
        // Use authenticated user name if available, otherwise fall back to circuit id or remote ip
        try
        {
            var user = _httpContextAccessor?.HttpContext?.User?.Identity?.Name;
            if (!string.IsNullOrEmpty(user)) return $"cart:user:{user}";
        }
        catch { }

        return "cart:anonymous"; // simple shared cart
    }

    private List<CartItemDto> GetOrCreateCart()
    {
        var key = GetCartKey();
        return _store.GetOrAdd(key, _ => new List<CartItemDto>());
    }

    public async Task AddItemAsync(Guid productId, int quantity, CancellationToken cancellationToken = default)
    {
        var product = await _productApi.GetProductAsync(productId);
        if (product == null) throw new InvalidOperationException("Product not found");

        var cart = GetOrCreateCart();
        var existing = cart.FirstOrDefault(x => x.ProductId == productId);
        if (existing != null)
        {
            existing.Quantity += quantity;
        }
        else
        {
            cart.Add(new CartItemDto
            {
                ProductId = productId,
                Name = product.Name,
                UnitPrice = product.Price,
                Quantity = quantity
            });
        }
    }

    public Task RemoveItemAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        var cart = GetOrCreateCart();
        cart.RemoveAll(x => x.ProductId == productId);
        return Task.CompletedTask;
    }

    public Task UpdateQuantityAsync(Guid productId, int quantity, CancellationToken cancellationToken = default)
    {
        var cart = GetOrCreateCart();
        var item = cart.FirstOrDefault(x => x.ProductId == productId);
        if (item != null)
        {
            if (quantity <= 0) cart.Remove(item);
            else item.Quantity = quantity;
        }
        return Task.CompletedTask;
    }

    public Task ClearAsync(CancellationToken cancellationToken = default)
    {
        var key = GetCartKey();
        _store.TryRemove(key, out _);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<CartItemDto>> GetItemsAsync(CancellationToken cancellationToken = default)
    {
        var cart = GetOrCreateCart();
        return Task.FromResult((IReadOnlyList<CartItemDto>)cart.ToList());
    }

    public Task<decimal> GetTotalAsync(CancellationToken cancellationToken = default)
    {
        var cart = GetOrCreateCart();
        var total = cart.Sum(i => i.UnitPrice * i.Quantity);
        return Task.FromResult(total);
    }
}
