using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CleanCut.WinApp.Services.Management
{
    /// <summary>
    /// File-based implementation of <see cref="ILayoutPersistenceService"/> storing layout JSON
    /// under user's AppData folder following the existing conventions.
    /// </summary>
    public class FileLayoutPersistenceService : ILayoutPersistenceService
    {
        public async Task<string?> LoadLayoutAsync(string moduleName, string appUserName, CancellationToken cancellationToken = default)
        {
            var prefsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CleanCut", appUserName);
            var filePath = Path.Combine(prefsPath, $"{moduleName}.layout.json");
            if (!File.Exists(filePath)) return null;

            return await File.ReadAllTextAsync(filePath, cancellationToken);
        }

        public async Task SaveLayoutAsync(string moduleName, string layoutJson, string appUserName, CancellationToken cancellationToken = default)
        {
            var prefsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CleanCut", appUserName);
            Directory.CreateDirectory(prefsPath);
            var filePath = Path.Combine(prefsPath, $"{moduleName}.layout.json");
            await File.WriteAllTextAsync(filePath, layoutJson, cancellationToken);
        }
    }
}
