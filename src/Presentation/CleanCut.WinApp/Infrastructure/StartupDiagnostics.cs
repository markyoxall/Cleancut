using System;
using System.IO;
using Serilog;

namespace CleanCut.WinApp.Infrastructure;

/// <summary>
/// Minimal startup diagnostics helpers used very early in application startup.
/// Implemented as a lightweight no-op safe helper so Program.ModuleInitialize and Main
/// can call Install/Uninstall without bringing in heavy dependencies.
/// </summary>
internal static class StartupDiagnostics
{
    private static string? _logFilePath;
    private static readonly object _sync = new();
    private static bool _installed;

    /// <summary>
    /// Install minimal diagnostics (writes an initial marker to the fallback crash file if possible).
    /// This method must be safe to call during module initialization.
    /// </summary>
    public static void Install(string? fallbackCrashFile)
    {
        // Make install idempotent and thread-safe: multiple early callers (module init + Main)
        // may invoke Install; only perform the work once.
        try
        {
            lock (_sync)
            {
                if (_installed)
                    return;

                _logFilePath = fallbackCrashFile ?? _logFilePath;

                if (!string.IsNullOrEmpty(_logFilePath))
                {
                    try
                    {
                        var dir = Path.GetDirectoryName(_logFilePath);
                        if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
                        File.AppendAllText(_logFilePath, $"[{DateTime.UtcNow:O}] StartupDiagnostics installed{Environment.NewLine}");
                    }
                    catch { /* best-effort only */ }
                }

                _installed = true;
            }
        }
        catch { }
    }

    /// <summary>
    /// Uninstall diagnostics (best-effort cleanup).
    /// </summary>
    public static void Uninstall()
    {
        try
        {
            lock (_sync)
            {
                if (!_installed)
                    return;

                if (!string.IsNullOrEmpty(_logFilePath))
                {
                    try
                    {
                        File.AppendAllText(_logFilePath, $"[{DateTime.UtcNow:O}] StartupDiagnostics uninstalled{Environment.NewLine}");
                    }
                    catch { }
                }

                // Reset state so Install can be called again in long-running test scenarios
                _installed = false;
                _logFilePath = null;
            }
        }
        catch { }
    }
}
