using CleanCut.Infrastructure.Caching.Constants;
using CleanCut.Application.Common.Interfaces;

namespace CleanCut.WinApp.Services.Caching;

/// <summary>
/// Default implementation of <see cref="ICacheManager"/> that orchestrates cache invalidation
/// by delegating to the application-level <see cref="ICacheService"/>.
/// 
/// SOLID mapping:
/// - SRP: single responsibility is to provide centralized invalidation methods.
/// - DIP: depends on the ICacheService abstraction instead of concrete cache types.
/// - LSP: implementations must behave as documented by <see cref="ICacheManager"/> (invalidate requested keys).
/// </summary>
public class CacheManager : ICacheManager
{
    private readonly ICacheService _cacheService;

    /// <summary>
    /// Create a new <see cref="CacheManager"/>.
    /// </summary>
    public CacheManager(CleanCut.Application.Common.Interfaces.ICacheService cacheService)
    {
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
    }

    /// <inheritdoc />
    public Task InvalidateCustomersAsync(CancellationToken cancellationToken = default)
    {
        // Implementation detail: pattern-based invalidation delegates to infrastructure (Redis) or memory.
        return _cacheService.RemoveByPatternAsync(CacheKeys.CustomerPattern(), cancellationToken);
    }

    /// <inheritdoc />
    public Task InvalidateProductsAsync(CancellationToken cancellationToken = default)
    {
        return _cacheService.RemoveByPatternAsync(CacheKeys.ProductPattern(), cancellationToken);
    }

    /// <inheritdoc />
    public Task InvalidateCountriesAsync(CancellationToken cancellationToken = default)
    {
        return _cacheService.RemoveByPatternAsync(CacheKeys.CountryPattern(), cancellationToken);
    }

    /// <inheritdoc />
    public Task InvalidateOrdersAsync(CancellationToken cancellationToken = default)
    {
        return _cacheService.RemoveByPatternAsync(CacheKeys.OrderPattern(), cancellationToken);
    }
}
