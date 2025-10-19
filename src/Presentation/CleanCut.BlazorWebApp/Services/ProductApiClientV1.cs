using CleanCut.Application.DTOs;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Threading;

namespace CleanCut.BlazorWebApp.Services;

public class ProductApiClientV1 : IProductApiClientV1
{
    private readonly HttpClient _http;
    private readonly ILogger<ProductApiClientV1> _logger;

    public ProductApiClientV1(HttpClient http, ILogger<ProductApiClientV1> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<List<ProductDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("V1: GET /api/v1/products");
        var resp = await _http.GetAsync("api/v1/products", cancellationToken);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync<List<ProductDto>>(cancellationToken: cancellationToken) ?? new();
    }

    public async Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("V1: GET /api/v1/products/{Id}", id);
        var resp = await _http.GetAsync($"api/v1/products/{id}", cancellationToken);
        if (resp.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync<ProductDto>(cancellationToken: cancellationToken);
    }

    public async Task<List<ProductDto>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("V1: GET /api/v1/products/user/{UserId}", userId);
        var resp = await _http.GetAsync($"api/v1/products/user/{userId}", cancellationToken);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync<List<ProductDto>>(cancellationToken: cancellationToken) ?? new();
    }

    public async Task<ProductDto> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("V1: POST /api/v1/products");
        var resp = await _http.PostAsJsonAsync("api/v1/products", request, cancellationToken);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync<ProductDto>(cancellationToken: cancellationToken)
               ?? throw new InvalidOperationException("Failed to create product (v1)");
    }

    public async Task<ProductDto> UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("V1: PUT /api/v1/products/{Id}", id);
        var resp = await _http.PutAsJsonAsync($"api/v1/products/{id}", request, cancellationToken);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync<ProductDto>(cancellationToken: cancellationToken)
               ?? throw new InvalidOperationException("Failed to update product (v1)");
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("V1: DELETE /api/v1/products/{Id}", id);
        var resp = await _http.DeleteAsync($"api/v1/products/{id}", cancellationToken);
        if (resp.StatusCode == System.Net.HttpStatusCode.NotFound) return false;
        resp.EnsureSuccessStatusCode();
        return resp.IsSuccessStatusCode;
    }
}