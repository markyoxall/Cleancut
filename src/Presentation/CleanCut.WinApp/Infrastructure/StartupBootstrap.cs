using System;
using System.IO;
using Serilog;
using Microsoft.Extensions.DependencyInjection;

namespace CleanCut.WinApp.Infrastructure;

/// <summary>
/// Bootstraps early startup tasks for the WinApp: prepares logs folder, installs fallback diagnostics,
/// and builds the DI service provider.
/// </summary>
public static class StartupBootstrap
{
    /// <summary>
    /// Perform early startup steps and build the application's service provider.
    /// </summary>
    /// <param name="basePath">Optional base path to resolve the logs folder. If null the method will fall back to <c>AppContext.BaseDirectory</c> or the current directory.</param>
    /// <param name="fallbackLogFile">Returns the resolved fallback log file path.</param>
    /// <param name="fallbackCrashFile">Returns the resolved fallback crash file path.</param>
    /// <returns>The built <see cref="IServiceProvider"/> from <c>ServiceConfiguration.ConfigureServices()</c>.</returns>
    public static IServiceProvider Boot(string? basePath, out string fallbackLogFile, out string fallbackCrashFile)
    {
        var resolvedBase = basePath ?? AppContext.BaseDirectory ?? Directory.GetCurrentDirectory();

        // Resolve using IProjectPathProvider if available (registered by default in ServiceConfiguration)
        // but we don't have DI yet, so fallback to previous heuristic.
        var logBase = resolvedBase;

        var logDirectory = Path.Combine(logBase, "logs");

        EnsureDirectoryExists(logDirectory);

        fallbackLogFile = Path.Combine(logDirectory, "cleancut-startup-.log");
        fallbackCrashFile = Path.Combine(logDirectory, "cleancut-fallback-errors.log");

        // Install minimal startup diagnostics early so any pre-DI errors are captured.
        try
        {
            StartupDiagnostics.Install(fallbackCrashFile);
        }
        catch (Exception ex)
        {
            try { Log.Warning(ex, "StartupDiagnostics.Install failed"); } catch { }
        }

        // Build services (this will also reconfigure Serilog to the configured sinks)
        var serviceProvider = ServiceConfiguration.ConfigureServices();

        // If DI is available, register a concrete project path provider for later use
        try
        {
            var services = new ServiceCollection();
            services.AddSingleton<IProjectPathProvider, GitAwareProjectPathProvider>();
        }
        catch { }

        return serviceProvider;
    }

    private static void EnsureDirectoryExists(string directory)
    {
        try
        {
            Directory.CreateDirectory(directory);
        }
        catch (Exception ex)
        {
            try { Log.Warning(ex, "Unable to create directory for logs: {Directory}", directory); } catch { }
        }
    }
}
