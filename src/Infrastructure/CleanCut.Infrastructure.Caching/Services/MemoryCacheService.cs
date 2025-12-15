using CleanCut.Infrastructure.Caching.Abstractions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CleanCut.Infrastructure.Caching.Services;

/// <summary>
/// In-memory implementation of caching service (fallback when Redis is not available)
/// </summary>
public class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<MemoryCacheService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;
    // Track keys set in the in-memory cache so we can support pattern removal.
    private readonly System.Collections.Concurrent.ConcurrentDictionary<string, byte> _keys = new();

    public MemoryCacheService(IMemoryCache memoryCache, ILogger<MemoryCacheService> logger)
    {
        _memoryCache = memoryCache;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            return Task.FromResult(_memoryCache.Get<T>(key));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cached value for key: {Key}", key);
            return Task.FromResult<T?>(null);
        }
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var options = new MemoryCacheEntryOptions();
            if (expiration.HasValue)
            {
                options.SetAbsoluteExpiration(expiration.Value);
            }
            else
            {
                // Default expiration of 1 hour
                options.SetAbsoluteExpiration(TimeSpan.FromHours(1));
            }

            _memoryCache.Set(key, value, options);
            // Track the key so pattern-based removals can work
            _keys.TryAdd(key, 0);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting cached value for key: {Key}", key);
            throw;
        }
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            _memoryCache.Remove(key);
            _keys.TryRemove(key, out _);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cached value for key: {Key}", key);
            throw;
        }
    }

    public Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        // Support simple glob-style patterns where '*' matches any sequence of characters.
        try
        {
            if (string.IsNullOrEmpty(pattern)) return Task.CompletedTask;

            // Convert glob to regex: escape regex special chars except '*', then replace '*' with '.*'
            var escaped = System.Text.RegularExpressions.Regex.Escape(pattern).Replace("\\*", "__AST__");
            var regexPattern = "^" + escaped.Replace("__AST__", ".*") + "$";
            var regex = new System.Text.RegularExpressions.Regex(regexPattern, System.Text.RegularExpressions.RegexOptions.Compiled | System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            var keysToRemove = _keys.Keys.Where(k => regex.IsMatch(k)).ToList();
            foreach (var k in keysToRemove)
            {
                _memoryCache.Remove(k);
                _keys.TryRemove(k, out _);
            }

            _logger.LogInformation("Removed {Count} in-memory cache entries by pattern {Pattern}", keysToRemove.Count, pattern);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error removing cache entries by pattern {Pattern}", pattern);
        }

        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            return Task.FromResult(_memoryCache.TryGetValue(key, out _));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if key exists: {Key}", key);
            return Task.FromResult(false);
        }
    }
}
