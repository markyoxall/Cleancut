using System.Threading;
using System.Threading.Tasks;

namespace CleanCut.WinApp.Services.Management
{
    /// <summary>
    /// Abstraction for loading and saving user preferences for a given module.
    /// 
    /// SOLID mapping:
    /// - SRP: single responsibility to load preferences from a chosen store.
    /// - DIP: callers depend on this abstraction, enabling implementations to change (file, DB) without modifying callers.
    /// - ISP: small, focused interface exposing only what's required by ManagementLoader.
    /// - LSP: implementations must return parsed preferences or null and not require extra preconditions.
    /// </summary>
    public interface IUserPreferencesService
    {
        /// <summary>
        /// Loads preferences for the specified module name.
        /// Returns <c>null</c> if no preferences are available.
        /// </summary>
        Task<UserPreferences?> LoadPreferencesAsync(string moduleName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Persists the provided preferences for the given module and application user.
        /// Implementations should either create or update the stored preferences.
        /// </summary>
        Task SavePreferencesAsync(string moduleName, UserPreferences preferences, string appUserName, CancellationToken cancellationToken = default);
    }
}
