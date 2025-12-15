using CleanCut.Application.DTOs;

namespace CleanCut.BlazorWebApp.Services;

public interface IOrdersApiService
{
    Task<OrderInfo> CreateOrderAsync(CreateOrderRequest request, CancellationToken cancellationToken = default);
    Task<OrderInfo?> GetOrderAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OrderInfo>> GetOrdersByCustomerAsync(Guid customerId, CancellationToken cancellationToken = default);
}

public record CreateOrderItemRequest(Guid ProductId, int Quantity);

public record CreateOrderRequest(Guid CustomerId, string ShippingAddress, string BillingAddress, IEnumerable<CreateOrderItemRequest>? Items = null, string? Notes = null);
