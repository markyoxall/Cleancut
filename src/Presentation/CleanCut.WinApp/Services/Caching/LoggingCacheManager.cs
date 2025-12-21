using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace CleanCut.WinApp.Services.Caching;

/// <summary>
/// Decorator around an <see cref="ICacheManager"/> that logs invalidation operations.
/// This class demonstrates the Decorator pattern and is provided to illustrate OCP in action:
/// new behavior (logging) is added without changing presenters or the concrete CacheManager.
/// </summary>
public class LoggingCacheManager : ICacheManager
{
    private readonly ICacheManager _inner;
    private readonly ILogger<LoggingCacheManager> _logger;

    public LoggingCacheManager(ICacheManager inner, ILogger<LoggingCacheManager> logger)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task InvalidateCustomersAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[Cache] InvalidateCustomersAsync called");
        await _inner.InvalidateCustomersAsync(cancellationToken);
        _logger.LogInformation("[Cache] InvalidateCustomersAsync completed");
    }

    public async Task InvalidateProductsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[Cache] InvalidateProductsAsync called");
        await _inner.InvalidateProductsAsync(cancellationToken);
        _logger.LogInformation("[Cache] InvalidateProductsAsync completed");
    }

    public async Task InvalidateCountriesAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[Cache] InvalidateCountriesAsync called");
        await _inner.InvalidateCountriesAsync(cancellationToken);
        _logger.LogInformation("[Cache] InvalidateCountriesAsync completed");
    }

    public async Task InvalidateOrdersAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[Cache] InvalidateOrdersAsync called");
        await _inner.InvalidateOrdersAsync(cancellationToken);
        _logger.LogInformation("[Cache] InvalidateOrdersAsync completed");
    }
}
