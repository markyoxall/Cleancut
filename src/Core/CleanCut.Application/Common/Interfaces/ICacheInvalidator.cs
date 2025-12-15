namespace CleanCut.Application.Common.Interfaces;

/// <summary>
/// Marker interface for commands that should invalidate cache
/// </summary>
public interface ICacheInvalidator
{
    /// <summary>
    /// Cache keys or patterns to invalidate
    /// </summary>
    IEnumerable<string> CacheKeysToInvalidate { get; }
}