using System.Threading;
using System.Threading.Tasks;

namespace CleanCut.Application.Common.Interfaces;

/// <summary>
/// Abstraction for publishing integration events to external systems (e.g. RabbitMQ, Redis pub/sub).
/// Implementations live in Infrastructure and are registered in DI.
/// </summary>
public interface IIntegrationEventPublisher
{
    /// <summary>
    /// Publish a strongly-typed integration event using the provided routing key/topic.
    /// This method should be best-effort and not throw for transient failures.
    /// </summary>
    Task PublishAsync(string routingKey, object payload, CancellationToken cancellationToken = default);
}
