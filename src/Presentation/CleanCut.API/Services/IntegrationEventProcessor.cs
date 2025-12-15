using CleanCut.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace CleanCut.API.Services;

/// <summary>
/// Thin orchestration service used by notification handlers to centralize common operations
/// such as cache invalidation, integration publish and SignalR broadcasting (SignalR performed by handlers directly via IHubContext).
/// Keeps handlers small and focused, follows SRP.
/// </summary>
public class IntegrationEventProcessor : IIntegrationEventProcessor
{
    private readonly IIntegrationEventPublisher _publisher;
    private readonly CleanCut.Application.Common.Interfaces.ICacheService _cacheService;
    private readonly ILogger<IntegrationEventProcessor> _logger;

    public IntegrationEventProcessor(IIntegrationEventPublisher publisher, CleanCut.Application.Common.Interfaces.ICacheService cacheService, ILogger<IntegrationEventProcessor> logger)
    {
        _publisher = publisher;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task ProcessAsync(string routingKey, object payload, CancellationToken cancellationToken = default)
    {
        // Invalidate cache by pattern where applicable (handlers may choose different patterns)
        try
        {
            // Best-effort generic invalidation for entity collections
            await _cacheService.RemoveByPatternAsync("*", cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Generic cache invalidation failed for routingKey={RoutingKey}", routingKey);
        }

        // Publish to integration bus
        try
        {
            await _publisher.PublishAsync(routingKey, payload, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Publishing integration event failed for routingKey={RoutingKey}", routingKey);
        }
    }
}
