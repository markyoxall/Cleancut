using CleanCut.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CleanCut.Application.Behaviors;

/// <summary>
/// Caching behavior for MediatR pipeline
/// </summary>
public class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<CachingBehavior<TRequest, TResponse>> _logger;

    public CachingBehavior(IDistributedCache cache, ILogger<CachingBehavior<TRequest, TResponse>> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        // Handle caching for queries
        if (request is ICacheableQuery cacheableQuery)
        {
            var cacheKey = cacheableQuery.CacheKey;
            
            // Try to get from cache first
            var cachedResponse = await GetFromCacheAsync<TResponse>(cacheKey, cancellationToken);
            if (cachedResponse != null)
            {
                _logger.LogInformation("Cache hit for key: {CacheKey}", cacheKey);
                return cachedResponse;
            }

            _logger.LogInformation("Cache miss for key: {CacheKey}", cacheKey);
            
            // Execute the handler
            var response = await next();
            
            // Cache the response (with error handling)
            await SetCacheAsync(cacheKey, response, cacheableQuery.Expiration, cancellationToken);
            
            return response;
        }

        // Handle cache invalidation for commands
        var response2 = await next();
        
        if (request is ICacheInvalidator cacheInvalidator)
        {
            foreach (var cacheKey in cacheInvalidator.CacheKeysToInvalidate)
            {
                await RemoveFromCacheAsync(cacheKey, cancellationToken);
            }
        }

        return response2;
    }

    private async Task<T?> GetFromCacheAsync<T>(string key, CancellationToken cancellationToken)
    {
        try
        {
            var cachedValue = await _cache.GetStringAsync(key, cancellationToken);
            if (string.IsNullOrEmpty(cachedValue))
                return default;

            return JsonSerializer.Deserialize<T>(cachedValue);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error getting value from cache for key: {Key}. Continuing without cache.", key);
            return default;
        }
    }

    private async Task SetCacheAsync<T>(string key, T value, TimeSpan? expiration, CancellationToken cancellationToken)
    {
        try
        {
            var serializedValue = JsonSerializer.Serialize(value);
            var options = new DistributedCacheEntryOptions();
            if (expiration.HasValue)
            {
                options.SetAbsoluteExpiration(expiration.Value);
            }
            else
            {
                options.SetAbsoluteExpiration(TimeSpan.FromHours(1));
            }

            await _cache.SetStringAsync(key, serializedValue, options, cancellationToken);
            _logger.LogDebug("Successfully cached value for key: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error setting cache for key: {Key}. Continuing without caching.", key);
        }
    }

    private async Task RemoveFromCacheAsync(string cacheKey, CancellationToken cancellationToken)
    {
        try
        {
            await _cache.RemoveAsync(cacheKey, cancellationToken);
            _logger.LogInformation("Cache invalidated for key: {CacheKey}", cacheKey);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error removing cache for key: {CacheKey}. Continuing without cache invalidation.", cacheKey);
        }
    }
}
