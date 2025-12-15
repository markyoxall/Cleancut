using System.Threading;
using CleanCut.Application.DTOs;
using CleanCut.BlazorWebApp.Services;
using Microsoft.Extensions.Logging;

namespace CleanCut.BlazorWebApp.State;

public class ProductsState : IProductsState
{
    private readonly IProductApiService _productApi;
    private readonly ILogger<ProductsState> _logger;

    private List<ProductInfo> _products = new();
    private DateTime _lastLoaded = DateTime.MinValue;
    private readonly TimeSpan _cacheExpiry = TimeSpan.FromMinutes(5);

    // UI state / messages
    private bool _isLoading;
    public bool IsLoading => _isLoading;
    public event Action<string, bool>? MessageChanged;

    public ProductsState(IProductApiService productApi, ILogger<ProductsState> logger)
    {
        _productApi = productApi;
        _logger = logger;
    }

    public IReadOnlyList<ProductInfo> Products => _products;
    public event Action? StateChanged;
    public event Action<List<ProductInfo>>? ProductsChanged;

    public async Task LoadAllAsync(bool force = false, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            throw new OperationCanceledException(cancellationToken);

        if (!force && DateTime.UtcNow - _lastLoaded < _cacheExpiry && _products.Any())
        {
            _logger.LogDebug("ProductsState: using cached products");
            return;
        }

        _isLoading = true;
        StateChanged?.Invoke();

        try
        {
            _logger.LogInformation("ProductsState: loading all products");
            _products = await _productApi.GetAllProductsAsync(cancellationToken);
            _lastLoaded = DateTime.UtcNow;

            var productsCopy = _products.ToList();
            ProductsChanged?.Invoke(productsCopy);
            StateChanged?.Invoke();

            _logger.LogInformation("ProductsState: loaded {Count} products", _products.Count);
            MessageChanged?.Invoke("Products loaded", true);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("ProductsState: LoadAllAsync canceled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ProductsState: error loading products");
            MessageChanged?.Invoke("Failed to load products", false);
            // swallow to keep UI stable; callers can react to MessageChanged
        }
        finally
        {
            _isLoading = false;
            StateChanged?.Invoke();
        }
    }

    public async Task LoadByCustomerAsync(Guid userId, bool force = false, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            throw new OperationCanceledException(cancellationToken);

        if (!force && DateTime.UtcNow - _lastLoaded < _cacheExpiry && _products.Any(p => p.CustomerId == userId))
        {
            _logger.LogDebug("ProductsState: using cached products for user {CustomerId}", userId);
            return;
        }

        _isLoading = true;
        StateChanged?.Invoke();

        try
        {
            _logger.LogInformation("ProductsState: loading products for user {CustomerId}", userId);
            var list = await _productApi.GetProductsByCustomerAsync(userId, cancellationToken);
            _products = list.ToList();
            _lastLoaded = DateTime.UtcNow;

            var productsCopy = _products.ToList();
            ProductsChanged?.Invoke(productsCopy);
            StateChanged?.Invoke();

            MessageChanged?.Invoke("Products loaded for user", true);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("ProductsState: LoadByCustomerAsync canceled for {CustomerId}", userId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ProductsState: error loading products for user {CustomerId}", userId);
            MessageChanged?.Invoke("Failed to load products for user", false);
            // swallow to keep UI stable; callers can react to MessageChanged
        }
        finally
        {
            _isLoading = false;
            StateChanged?.Invoke();
        }
    }

    public async Task<ProductInfo?> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            throw new OperationCanceledException(cancellationToken);

        _isLoading = true;
        StateChanged?.Invoke();
        try
        {
            _logger.LogInformation("ProductsState: creating product {Name}", request.Name);
            var created = await _productApi.CreateProductAsync(request, cancellationToken);
            _products.Add(created);

            var productsCopy = _products.ToList();
            ProductsChanged?.Invoke(productsCopy);
            StateChanged?.Invoke();
            MessageChanged?.Invoke($"Product '{created.Name}' created", true);
            return created;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("ProductsState: CreateAsync canceled for {Name}", request.Name);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ProductsState: error creating product");
            MessageChanged?.Invoke("Failed to create product", false);
            return null;
        }
        finally
        {
            _isLoading = false;
            StateChanged?.Invoke();
        }
    }

    public async Task<ProductInfo?> UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            throw new OperationCanceledException(cancellationToken);

        _isLoading = true;
        StateChanged?.Invoke();
        try
        {
            _logger.LogInformation("ProductsState: updating product {ProductId}", id);
            var updated = await _productApi.UpdateProductAsync(id, request, cancellationToken);
            var idx = _products.FindIndex(p => p.Id == id);
            if (idx >= 0) _products[idx] = updated;

            var productsCopy = _products.ToList();
            ProductsChanged?.Invoke(productsCopy);
            StateChanged?.Invoke();
            MessageChanged?.Invoke($"Product '{updated.Name}' updated", true);
            return updated;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("ProductsState: UpdateAsync canceled for {ProductId}", id);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ProductsState: error updating product {ProductId}", id);
            MessageChanged?.Invoke("Failed to update product", false);
            return null;
        }
        finally
        {
            _isLoading = false;
            StateChanged?.Invoke();
        }
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            throw new OperationCanceledException(cancellationToken);

        _isLoading = true;
        StateChanged?.Invoke();
        try
        {
            _logger.LogInformation("ProductsState: deleting product {ProductId}", id);
            var success = await _productApi.DeleteProductAsync(id, cancellationToken);
            if (success)
            {
                _products.RemoveAll(p => p.Id == id);

                var productsCopy = _products.ToList();
                ProductsChanged?.Invoke(productsCopy);
                StateChanged?.Invoke();
                MessageChanged?.Invoke("Product deleted", true);
            }
            return success;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("ProductsState: DeleteAsync canceled for {ProductId}", id);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ProductsState: error deleting product {ProductId}", id);
            MessageChanged?.Invoke("Failed to delete product", false);
            return false;
        }
        finally
        {
            _isLoading = false;
            StateChanged?.Invoke();
        }
    }

    public void Invalidate()
    {
        _lastLoaded = DateTime.MinValue;
        StateChanged?.Invoke();
    }
}