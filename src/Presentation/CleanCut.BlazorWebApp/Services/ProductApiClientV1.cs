using CleanCut.Application.DTOs;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading;

namespace CleanCut.BlazorWebApp.Services;

public class ProductApiClientV1 : IProductApiClientV1
{
    private readonly HttpClient _http;
    private readonly ILogger<ProductApiClientV1> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ProductApiClientV1(HttpClient http, ILogger<ProductApiClientV1> logger, IHttpContextAccessor httpContextAccessor)
    {
        _http = http;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    private async Task AttachAccessTokenAsync()
    {
        var accessToken = await _httpContextAccessor.HttpContext.GetTokenAsync("access_token");
        if (!string.IsNullOrEmpty(accessToken))
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }
    }

    public async Task<List<ProductInfo>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await AttachAccessTokenAsync();
        _logger.LogDebug("V1: GET /api/v1/products");
        var resp = await _http.GetAsync("api/v1/products", cancellationToken);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync<List<ProductInfo>>(cancellationToken: cancellationToken) ?? new();
    }

    public async Task<ProductInfo?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await AttachAccessTokenAsync();
        _logger.LogDebug("V1: GET /api/v1/products/{Id}", id);
        var resp = await _http.GetAsync($"api/v1/products/{id}", cancellationToken);
        if (resp.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync<ProductInfo>(cancellationToken: cancellationToken);
    }

    public async Task<List<ProductInfo>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        await AttachAccessTokenAsync();
        _logger.LogDebug("V1: GET /api/v1/products/user/{UserId}", userId);
        var resp = await _http.GetAsync($"api/v1/products/user/{userId}", cancellationToken);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync<List<ProductInfo>>(cancellationToken: cancellationToken) ?? new();
    }

    public async Task<ProductInfo> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default)
    {
        await AttachAccessTokenAsync();
        _logger.LogDebug("V1: POST /api/v1/products");
        var resp = await _http.PostAsJsonAsync("api/v1/products", request, cancellationToken);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync<ProductInfo>(cancellationToken: cancellationToken)
               ?? throw new InvalidOperationException("Failed to create product (v1)");
    }

    public async Task<ProductInfo> UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken = default)
    {
        await AttachAccessTokenAsync();
        _logger.LogDebug("V1: PUT /api/v1/products/{Id}", id);
        var resp = await _http.PutAsJsonAsync($"api/v1/products/{id}", request, cancellationToken);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync<ProductInfo>(cancellationToken: cancellationToken)
               ?? throw new InvalidOperationException("Failed to update product (v1)");
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await AttachAccessTokenAsync();
        _logger.LogDebug("V1: DELETE /api/v1/products/{Id}", id);
        var resp = await _http.DeleteAsync($"api/v1/products/{id}", cancellationToken);
        if (resp.StatusCode == System.Net.HttpStatusCode.NotFound) return false;
        resp.EnsureSuccessStatusCode();
        return resp.IsSuccessStatusCode;
    }
}