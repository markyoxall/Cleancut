using MediatR;
using CleanCut.Application.DTOs;
using CleanCut.Application.Common.Interfaces;

namespace CleanCut.Application.Commands.Orders.AddOrderLineItem;

/// <summary>
/// Command to add a line item to an order
/// </summary>
public record AddOrderLineItemCommand(
    Guid OrderId,
    Guid ProductId,
    int Quantity
) : IRequest<OrderInfo>, ICacheInvalidator
{
    public IEnumerable<string> CacheKeysToInvalidate => 
        ["order:all", $"order:{OrderId}"];
}