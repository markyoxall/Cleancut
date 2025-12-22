using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace CleanCut.WinApp.Services.Management
{
    /// <summary>
    /// File-based implementation of <see cref="IUserPreferencesService"/> that reads
    /// preferences JSON from the user's application data folder.
    /// 
    /// SOLID mapping:
    /// - SRP: single responsibility reading preferences from disk.
    /// - DIP: implements the abstraction so callers remain decoupled from the file system.
    /// - LSP: adheres to the contract of returning <see cref="UserPreferences"/> or null and not requiring callers to do extra steps.
    /// </summary>
    public class UserPreferencesService : IUserPreferencesService
    {
        private readonly ILogger<UserPreferencesService> _logger;

        public UserPreferencesService(ILogger<UserPreferencesService> logger)
        {
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<UserPreferences?> LoadPreferencesAsync(string moduleName, string appUserName, CancellationToken cancellationToken = default)
        {
            string prefsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "CleanCut",
                appUserName,
                $"{moduleName}.prefs.json");

            if (!File.Exists(prefsPath)) return null;

            try
            {
                string json = await File.ReadAllTextAsync(prefsPath, cancellationToken);
                return System.Text.Json.JsonSerializer.Deserialize<UserPreferences>(json);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load preferences for {Module} and user {User}", moduleName, appUserName);
                return null;
            }
        }

        /// <inheritdoc />
        public async Task SavePreferencesAsync(string moduleName, UserPreferences preferences, string appUserName, CancellationToken cancellationToken = default)
        {
            string prefsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "CleanCut",
                appUserName,
                $"{moduleName}.prefs.json");

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(prefsPath)!);
                var json = System.Text.Json.JsonSerializer.Serialize(preferences, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(prefsPath, json, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to save preferences for {Module}", moduleName);
                throw;
            }
        }
    }
}
