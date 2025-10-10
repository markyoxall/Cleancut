namespace CleanCut.Application.Common.Interfaces;

/// <summary>
/// Marker interface for queries that should be cached
/// </summary>
public interface ICacheableQuery
{
    /// <summary>
    /// Cache key for this query
    /// </summary>
    string CacheKey { get; }
    
    /// <summary>
    /// Cache expiration time
    /// </summary>
    TimeSpan? Expiration { get; }
}