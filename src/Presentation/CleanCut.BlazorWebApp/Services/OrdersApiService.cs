using CleanCut.Application.DTOs;
using System.Net.Http.Json;

namespace CleanCut.BlazorWebApp.Services;

public class OrdersApiService : IOrdersApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OrdersApiService> _logger;

    public OrdersApiService(HttpClient httpClient, ILogger<OrdersApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<OrderInfo> CreateOrderAsync(CreateOrderRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/orders", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        var order = await response.Content.ReadFromJsonAsync<OrderInfo>(cancellationToken: cancellationToken);
        return order ?? throw new InvalidOperationException("Failed to create order");
    }

    public async Task<OrderInfo?> GetOrderAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"/api/orders/{id}", cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<OrderInfo>(cancellationToken: cancellationToken);
    }

    public async Task<IReadOnlyList<OrderInfo>> GetOrdersByCustomerAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"/api/orders/customer/{customerId}", cancellationToken);
        response.EnsureSuccessStatusCode();
        var list = await response.Content.ReadFromJsonAsync<List<OrderInfo>>(cancellationToken: cancellationToken) ?? new List<OrderInfo>();
        return list;
    }
}
