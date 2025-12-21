using System.Threading;
using System.Threading.Tasks;

namespace CleanCut.WinApp.Services.Caching;

/// <summary>
/// Abstraction for cache invalidation operations used by presenters and loaders.
/// 
/// SOLID notes:
/// - Single Responsibility (SRP): the interface has a single responsibility — provide cache invalidation operations.
/// - Interface Segregation (ISP): small focused interface so consumers depend only on what they need.
/// - Dependency Inversion (DIP): callers depend on this abstraction rather than concrete cache implementations.
/// - Liskov Substitution (LSP): implementations must honor the contract (invalidate related cache keys) so they are substitutable.
/// </summary>
public interface ICacheManager
{
    /// <summary>
    /// Invalidate all customer-related cache entries.
    /// Implementations should remove or expire any cache keys related to customers.
    /// </summary>
    Task InvalidateCustomersAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalidate all product-related cache entries.
    /// </summary>
    Task InvalidateProductsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalidate all country-related cache entries.
    /// </summary>
    Task InvalidateCountriesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalidate all order-related cache entries.
    /// </summary>
    Task InvalidateOrdersAsync(CancellationToken cancellationToken = default);
}
