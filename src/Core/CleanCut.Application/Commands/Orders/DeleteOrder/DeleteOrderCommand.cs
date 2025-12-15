using MediatR;
using CleanCut.Application.Common.Interfaces;

namespace CleanCut.Application.Commands.Orders.DeleteOrder;

/// <summary>
/// Command to delete an order
/// </summary>
public record DeleteOrderCommand(Guid Id) : IRequest<bool>, ICacheInvalidator
{
    public IEnumerable<string> CacheKeysToInvalidate => 
        ["order:all", $"order:{Id}"];
}