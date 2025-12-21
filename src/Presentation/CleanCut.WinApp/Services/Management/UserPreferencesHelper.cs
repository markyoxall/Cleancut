using System.Text.Json;
using System.IO;

namespace CleanCut.WinApp.Services.Management;

public static class UserPreferencesHelper
{
    public static async Task SavePreferencesAsync(string moduleName, UserPreferences preferences, string appUserName)
    {
        string prefsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "CleanCut",
            appUserName,
            $"{moduleName}.prefs.json");
        Directory.CreateDirectory(Path.GetDirectoryName(prefsPath)!);
        var json = JsonSerializer.Serialize(preferences, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(prefsPath, json);
    }
}
