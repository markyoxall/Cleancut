using MediatR;
using CleanCut.Application.DTOs;
using CleanCut.Application.Common.Interfaces;

namespace CleanCut.Application.Commands.Orders.CreateOrder;

/// <summary>
/// Command to create a new order
/// </summary>
public record CreateOrderItem(Guid ProductId, int Quantity);

/// <summary>
/// Command to create a new order
/// </summary>
public record CreateOrderCommand(
    Guid CustomerId,
    string ShippingAddress,
    string BillingAddress,
    IEnumerable<CreateOrderItem>? Items = null,
    string? Notes = null
) : IRequest<OrderInfo>, ICacheInvalidator
{
    public IEnumerable<string> CacheKeysToInvalidate => 
        ["order:all", $"order:customer:{CustomerId}"];
}
