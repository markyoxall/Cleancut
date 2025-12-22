using System.Threading;
using System.Threading.Tasks;

namespace CleanCut.WinApp.Services.Management
{
    /// <summary>
    /// Abstraction for loading and saving user preferences for a given module.
    /// </summary>
    public interface IUserPreferencesService
    {
        /// <summary>
        /// Loads preferences for the specified module name and application user.
        /// Returns <c>null</c> if no preferences are available.
        /// </summary>
        Task<UserPreferences?> LoadPreferencesAsync(string moduleName, string appUserName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Persists the provided preferences for the given module and application user.
        /// </summary>
        Task SavePreferencesAsync(string moduleName, UserPreferences preferences, string appUserName, CancellationToken cancellationToken = default);
    }
}
