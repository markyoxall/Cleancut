using CleanCut.Application.DTOs;
using Microsoft.Extensions.Logging;
using System.Threading;

namespace CleanCut.BlazorWebApp.Services;

// Adapter used by Blazor UI — delegates to typed v1/v2 clients.
// NOTE: V2 DTO types live in ProductApiV2Dtos.cs only.
public interface IProductApiService
{
    // v1 (simple) methods
    Task<List<ProductInfo>> GetAllProductsAsync(CancellationToken cancellationToken = default);
    Task<ProductInfo?> GetProductByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ProductInfo?> GetProductAsync(Guid id, CancellationToken cancellationToken = default); // helper for shopping cart
    Task<IEnumerable<ProductInfo>> GetProductsByCustomerAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<ProductInfo> CreateProductAsync(CreateProductRequest request, CancellationToken cancellationToken = default);
    Task<ProductInfo> UpdateProductAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteProductAsync(Guid id, CancellationToken cancellationToken = default);

    // explicit v2 methods (no ambiguous overloads)
    Task<V2ProductListResponse> GetAllProductsV2Async(int page = 1, int pageSize = 10, CancellationToken cancellationToken = default);
    Task<ProductInfo?> GetProductByIdV2Async(Guid id, CancellationToken cancellationToken = default);
    Task<V2ProductListResponse> GetProductsByCustomerV2Async(Guid userId, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default);
    Task<V2StatsResponse> GetProductStatisticsV2Async(CancellationToken cancellationToken = default);
}

public class ProductApiService : IProductApiService
{
    private readonly IProductApiClientV1 _v1;
    private readonly IProductApiClientV2 _v2;
    private readonly ILogger<ProductApiService> _logger;

    public ProductApiService(IProductApiClientV1 v1, IProductApiClientV2 v2, ILogger<ProductApiService> logger)
    {
        _v1 = v1;
        _v2 = v2;
        _logger = logger;
    }

    // v1 delegations
    public Task<List<ProductInfo>> GetAllProductsAsync(CancellationToken cancellationToken = default) => _v1.GetAllAsync(cancellationToken);
    public Task<ProductInfo?> GetProductByIdAsync(Guid id, CancellationToken cancellationToken = default) => _v1.GetByIdAsync(id, cancellationToken);
    public async Task<ProductInfo?> GetProductAsync(Guid id, CancellationToken cancellationToken = default) => await _v1.GetByIdAsync(id, cancellationToken);
    public Task<IEnumerable<ProductInfo>> GetProductsByCustomerAsync(Guid userId, CancellationToken cancellationToken = default) => _v1.GetByCustomerAsync(userId, cancellationToken).ContinueWith(t => (IEnumerable<ProductInfo>)t.Result, cancellationToken);
    public Task<ProductInfo> CreateProductAsync(CreateProductRequest request, CancellationToken cancellationToken = default) => _v1.CreateAsync(request, cancellationToken);
    public Task<ProductInfo> UpdateProductAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken = default) => _v1.UpdateAsync(id, request, cancellationToken);
    public Task<bool> DeleteProductAsync(Guid id, CancellationToken cancellationToken = default) => _v1.DeleteAsync(id, cancellationToken);

    // v2 delegations
    public Task<V2ProductListResponse> GetAllProductsV2Async(int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        => _v2.GetAllAsync(page, pageSize, cancellationToken);

    public async Task<ProductInfo?> GetProductByIdV2Async(Guid id, CancellationToken cancellationToken = default)
    {
        var wrapper = await _v2.GetByIdAsync(id, cancellationToken);
        return wrapper?.Data;
    }

    public Task<V2ProductListResponse> GetProductsByCustomerV2Async(Guid userId, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        => _v2.GetByCustomerAsync(userId, page, pageSize, cancellationToken);

    public Task<V2StatsResponse> GetProductStatisticsV2Async(CancellationToken cancellationToken = default)
        => _v2.GetStatisticsAsync(cancellationToken);
}
