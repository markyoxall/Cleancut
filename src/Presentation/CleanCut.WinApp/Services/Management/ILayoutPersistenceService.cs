using System.Threading;
using System.Threading.Tasks;

namespace CleanCut.WinApp.Services.Management
{
    /// <summary>
    /// Abstraction for persisting and retrieving grid/layout JSON for a given module and application user.
    /// Implementations should store layout JSON in a testable, swappable manner (file, database, etc.).
    /// </summary>
    public interface ILayoutPersistenceService
    {
        /// <summary>
        /// Load the layout JSON for the specified module and user. Returns null if no layout exists.
        /// </summary>
        Task<string?> LoadLayoutAsync(string moduleName, string appUserName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Save the provided layout JSON for the specified module and user.
        /// </summary>
        Task SaveLayoutAsync(string moduleName, string layoutJson, string appUserName, CancellationToken cancellationToken = default);
    }
}
