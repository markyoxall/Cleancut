using CleanCut.Application.DTOs;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Threading;

namespace CleanCut.BlazorWebApp.Services;

public class ProductApiClientV2 : IProductApiClientV2
{
    private readonly HttpClient _http;
    private readonly ILogger<ProductApiClientV2> _logger;

    public ProductApiClientV2(HttpClient http, ILogger<ProductApiClientV2> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<V2ProductListResponse> GetAllAsync(int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("V2: GET /api/v2/products?page={Page}&pageSize={PageSize}", page, pageSize);
        var resp = await _http.GetAsync($"api/v2/products?page={page}&pageSize={pageSize}", cancellationToken);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync<V2ProductListResponse>(cancellationToken: cancellationToken)
               ?? new V2ProductListResponse { Data = new List<ProductInfo>(), Pagination = new PaginationInfo { Page = page, PageSize = pageSize }, ApiVersion = "v2", Timestamp = DateTime.UtcNow };
    }

    public async Task<V2ProductResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("V2: GET /api/v2/products/{Id}", id);
        var resp = await _http.GetAsync($"api/v2/products/{id}", cancellationToken);
        if (resp.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync<V2ProductResponse>(cancellationToken: cancellationToken);
    }

    public async Task<V2ProductListResponse> GetByCustomerAsync(Guid customerId, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("V2: GET /api/v2/products/customer/{CustomerId}?page={Page}&pageSize={PageSize}", customerId, page, pageSize);
        var resp = await _http.GetAsync($"api/v2/products/customer/{customerId}?page={page}&pageSize={pageSize}", cancellationToken);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync<V2ProductListResponse>(cancellationToken: cancellationToken)
               ?? new V2ProductListResponse { Data = new List<ProductInfo>(), Pagination = new PaginationInfo { Page = page, PageSize = pageSize }, ApiVersion = "v2", Timestamp = DateTime.UtcNow };
    }

    public async Task<V2StatsResponse> GetStatisticsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("V2: GET /api/v2/products/statistics");
        var resp = await _http.GetAsync("api/v2/products/statistics", cancellationToken);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync<V2StatsResponse>(cancellationToken: cancellationToken)
               ?? new V2StatsResponse { Data = new StatsData(), ApiVersion = "v2", Timestamp = DateTime.UtcNow };
    }
}