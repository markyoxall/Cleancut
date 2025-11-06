using MediatR;
using CleanCut.Application.DTOs;
using CleanCut.Application.Common.Interfaces;

namespace CleanCut.Application.Commands.Orders.UpdateOrder;

/// <summary>
/// Command to update an existing order
/// </summary>
public record UpdateOrderCommand(
    Guid Id,
    string ShippingAddress,
    string BillingAddress,
    string? Notes = null
) : IRequest<OrderInfo>, ICacheInvalidator
{
    public IEnumerable<string> CacheKeysToInvalidate => 
        ["order:all", $"order:{Id}"];
}