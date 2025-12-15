using CleanCut.Application.DTOs;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
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

    public async Task<List<ProductInfo>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("V1: GET /api/v1/products");

        try
        {
            var resp = await _http.GetAsync("api/v1/products", cancellationToken);
            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadFromJsonAsync<List<ProductInfo>>(cancellationToken: cancellationToken) ?? new();
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("401") || ex.Message.Contains("Unauthorized"))
        {
            _logger.LogWarning("Unauthorized request - check token validity");
            throw new UnauthorizedAccessException("Token may be expired or invalid", ex);
        }
    }

    public async Task<ProductInfo?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("V1: GET /api/v1/products/{Id}", id);

        try
        {
            var resp = await _http.GetAsync($"api/v1/products/{id}", cancellationToken);
            if (resp.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadFromJsonAsync<ProductInfo>(cancellationToken: cancellationToken);
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("401") || ex.Message.Contains("Unauthorized"))
        {
            _logger.LogWarning("Unauthorized request - check token validity");
            throw new UnauthorizedAccessException("Token may be expired or invalid", ex);
        }
    }

    public async Task<List<ProductInfo>> GetByCustomerAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("V1: GET /api/v1/products/customer/{CustomerId}", customerId);

        try
        {
            // FIXED: Use customer endpoint instead of user endpoint
            var resp = await _http.GetAsync($"api/v1/products/customer/{customerId}", cancellationToken);
            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadFromJsonAsync<List<ProductInfo>>(cancellationToken: cancellationToken) ?? new();
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("401") || ex.Message.Contains("Unauthorized"))
        {
            _logger.LogWarning("Unauthorized request - check token validity");
            throw new UnauthorizedAccessException("Token may be expired or invalid", ex);
        }
    }

    public async Task<ProductInfo> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("V1: POST /api/v1/products");

        try
        {
            var resp = await _http.PostAsJsonAsync("api/v1/products", request, cancellationToken);
            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadFromJsonAsync<ProductInfo>(cancellationToken: cancellationToken)
               ?? throw new InvalidOperationException("Failed to create product (v1)");
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("401") || ex.Message.Contains("Unauthorized"))
        {
            _logger.LogWarning("Unauthorized request - check token validity");
            throw new UnauthorizedAccessException("Token may be expired or invalid", ex);
        }
    }

    public async Task<ProductInfo> UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("V1: PUT /api/v1/products/{Id}", id);

        try
        {
            var resp = await _http.PutAsJsonAsync($"api/v1/products/{id}", request, cancellationToken);
            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadFromJsonAsync<ProductInfo>(cancellationToken: cancellationToken)
             ?? throw new InvalidOperationException("Failed to update product (v1)");
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("401") || ex.Message.Contains("Unauthorized"))
        {
            _logger.LogWarning("Unauthorized request - check token validity");
            throw new UnauthorizedAccessException("Token may be expired or invalid", ex);
        }
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("V1: DELETE /api/v1/products/{Id}", id);

        try
        {
            var resp = await _http.DeleteAsync($"api/v1/products/{id}", cancellationToken);
            if (resp.StatusCode == System.Net.HttpStatusCode.NotFound) return false;
            resp.EnsureSuccessStatusCode();
            return resp.IsSuccessStatusCode;
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("401") || ex.Message.Contains("Unauthorized"))
        {
            _logger.LogWarning("Unauthorized request - check token validity");
            throw new UnauthorizedAccessException("Token may be expired or invalid", ex);
        }
    }
}
