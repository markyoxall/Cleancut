using CleanCut.Application.Common.Interfaces;

namespace CleanCut.Infrastructure.Caching.Adapters;

public class CacheServiceAdapter : CleanCut.Application.Common.Interfaces.ICacheService
{
    private readonly CleanCut.Infrastructure.Caching.Abstractions.ICacheService _inner;

    public CacheServiceAdapter(CleanCut.Infrastructure.Caching.Abstractions.ICacheService inner)
    {
        _inner = inner;
    }

    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
        => _inner.GetAsync<T>(key, cancellationToken);

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class
        => _inner.SetAsync(key, value, expiration, cancellationToken);

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        => _inner.RemoveAsync(key, cancellationToken);

    public Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
        => _inner.RemoveByPatternAsync(pattern, cancellationToken);

    public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
        => _inner.ExistsAsync(key, cancellationToken);
}
