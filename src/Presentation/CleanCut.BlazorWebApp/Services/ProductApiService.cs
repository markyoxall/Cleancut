using CleanCut.Application.DTOs;
using Microsoft.Extensions.Logging;
using System.Threading;

namespace CleanCut.BlazorWebApp.Services;

// Adapter used by Blazor UI — delegates to typed v1/v2 clients.
// NOTE: V2 DTO types live in ProductApiV2Dtos.cs only.
public interface IProductApiService
{
    // v1 (simple) methods
    Task<List<ProductDto>> GetAllProductsAsync(CancellationToken cancellationToken = default);
    Task<ProductDto?> GetProductByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProductDto>> GetProductsByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<ProductDto> CreateProductAsync(CreateProductRequest request, CancellationToken cancellationToken = default);
    Task<ProductDto> UpdateProductAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteProductAsync(Guid id, CancellationToken cancellationToken = default);

    // explicit v2 methods (no ambiguous overloads)
    Task<V2ProductListResponse> GetAllProductsV2Async(int page = 1, int pageSize = 10, CancellationToken cancellationToken = default);
    Task<ProductDto?> GetProductByIdV2Async(Guid id, CancellationToken cancellationToken = default);
    Task<V2ProductListResponse> GetProductsByUserV2Async(Guid userId, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default);
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
    public Task<List<ProductDto>> GetAllProductsAsync(CancellationToken cancellationToken = default) => _v1.GetAllAsync(cancellationToken);
    public Task<ProductDto?> GetProductByIdAsync(Guid id, CancellationToken cancellationToken = default) => _v1.GetByIdAsync(id, cancellationToken);
    public Task<IEnumerable<ProductDto>> GetProductsByUserAsync(Guid userId, CancellationToken cancellationToken = default) => _v1.GetByUserAsync(userId, cancellationToken).ContinueWith(t => (IEnumerable<ProductDto>)t.Result, cancellationToken);
    public Task<ProductDto> CreateProductAsync(CreateProductRequest request, CancellationToken cancellationToken = default) => _v1.CreateAsync(request, cancellationToken);
    public Task<ProductDto> UpdateProductAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken = default) => _v1.UpdateAsync(id, request, cancellationToken);
    public Task<bool> DeleteProductAsync(Guid id, CancellationToken cancellationToken = default) => _v1.DeleteAsync(id, cancellationToken);

    // v2 delegations
    public Task<V2ProductListResponse> GetAllProductsV2Async(int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        => _v2.GetAllAsync(page, pageSize, cancellationToken);

    public async Task<ProductDto?> GetProductByIdV2Async(Guid id, CancellationToken cancellationToken = default)
    {
        var wrapper = await _v2.GetByIdAsync(id, cancellationToken);
        return wrapper?.Data;
    }

    public Task<V2ProductListResponse> GetProductsByUserV2Async(Guid userId, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        => _v2.GetByUserAsync(userId, page, pageSize, cancellationToken);

    public Task<V2StatsResponse> GetProductStatisticsV2Async(CancellationToken cancellationToken = default)
        => _v2.GetStatisticsAsync(cancellationToken);
}