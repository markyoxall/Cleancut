using CleanCut.Application.DTOs;

namespace CleanCut.Application.Common.Interfaces;

public interface IRabbitMqPublisher
{
    /// <summary>
    /// Publish an OrderCreated message. If publishing fails, implementation may queue for retry.
    /// </summary>
    Task PublishOrderCreatedAsync(OrderInfo order, CancellationToken cancellationToken = default);

    /// <summary>
    /// Try to publish an OrderCreated message synchronously. Returns true on success, false on failure.
    /// Does NOT enqueue for retry - intended for background retry worker.
    /// </summary>
    Task<bool> TryPublishOrderCreatedAsync(OrderInfo order, CancellationToken cancellationToken = default);
}
