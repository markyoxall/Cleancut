using CleanCut.Application.DTOs;

namespace CleanCut.Application.Common.Interfaces;

public interface IRabbitMqRetryQueue
{
    Task EnqueueAsync(OrderInfo order, CancellationToken cancellationToken = default);
    Task<OrderInfo?> DequeueAsync(CancellationToken cancellationToken = default);
}
