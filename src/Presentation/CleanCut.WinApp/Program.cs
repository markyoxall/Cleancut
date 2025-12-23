using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using CleanCut.WinApp.Infrastructure;
using System.Runtime.CompilerServices;
using Serilog;
using Microsoft.Extensions.DependencyInjection;
using CleanCut.WinApp.Services;


namespace CleanCut.WinApp;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    private static string? _fallbackLogFile;
    private static string? _fallbackCrashFile;
 

    [STAThread]
    static async Task Main()
    {

        try
        {
            // To customize application configuration such as set high DPI settings or default font,
            // perform explicit initialization steps to avoid relying on the newer
            // `ApplicationConfiguration` helper which may not be present in all builds.
            ApplicationConfiguration.Initialize();

            // Install minimal fallback diagnostic handlers that run before DI is configured
            // Use the project path provider directly (no DI yet) to keep Program tidy
            var basePath = new GitAwareProjectPathProvider().GetProjectBasePath() ?? AppContext.BaseDirectory ?? Directory.GetCurrentDirectory();

            // Bootstrap startup tasks into a single helper so Main stays tidy
            var serviceProvider = StartupBootstrap.Boot(basePath, out _fallbackLogFile, out _fallbackCrashFile);

            // Install global exception handlers via helper class to keep Main tidy
            var globalHandlers = new GlobalExceptionHandlers(serviceProvider);
            if (!globalHandlers.Install())
            {
                // If installation failed, abort startup - Install shows UI to user already
                return;
            }

            // Get logger from DI to produce structured startup messages
            var loggerFactory = serviceProvider.GetRequiredService<Microsoft.Extensions.Logging.ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("CleanCut.WinApp.Program");
            logger.LogInformation("Starting CleanCut WinApp application");
            logger.LogInformation("Working directory: {cwd}", Directory.GetCurrentDirectory());

            // Create and run main form
            var mainForm = serviceProvider.GetRequiredService<MainForm>();
            System.Windows.Forms.Application.Run(mainForm);

            logger.LogInformation("CleanCut WinApp application stopped");

            // Unsubscribe handlers on graceful shutdown
            try
            {
                // Uninstall global handlers
                try { globalHandlers.Uninstall(); } catch { }
            }
            catch { }

            // Uninstall fallback diagnostics
            try { StartupDiagnostics.Uninstall(); } catch { }
        }
        catch (Exception ex)
        {
            // Prefer Serilog if available, fall back to MessageBox so user sees error
            try
            {
                Log.Fatal(ex, "Application terminated unexpectedly");
            }
            catch { }

            try
            {
                DeveloperDialog.ShowFatal("Fatal Error", $"A fatal error occurred: {ex.Message}");
            }
            catch { }
        }
        finally
        {
            try { Log.CloseAndFlush(); } catch { }
        }
    }



    // Module initializer runs as early as possible when the assembly is loaded.
    // Configure a minimal fallback logger here only; heavy fallback handlers are installed from Main
    [ModuleInitializer]
    public static void ModuleInitialize()
    {
        try
        {
            // Resolve base path using the same resolver used at runtime. We cannot use DI here,
            // so instantiate the provider directly. Keep this initializer minimal to capture
            // pre-Main failures only.
            string basePath = new GitAwareProjectPathProvider().GetProjectBasePath() ?? AppContext.BaseDirectory ?? Directory.GetCurrentDirectory();

            var logDirectory = Path.Combine(basePath, "logs");
            _fallbackLogFile = Path.Combine(logDirectory, "cleancut-startup-.log");
            _fallbackCrashFile = Path.Combine(logDirectory, "cleancut-fallback-errors.log");


            try
            {
                // Minimal Serilog configuration so early failures are captured to console and a fallback file
                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Debug()
                    .WriteTo.Console()
                    .WriteTo.File(_fallbackLogFile, rollingInterval: RollingInterval.Day)
                    .CreateLogger();
            }
            catch { }

            try
            {
                // Install startup diagnostics as early as possible so exceptions during module init are captured
                StartupDiagnostics.Install(_fallbackCrashFile);
            }
            catch { }
        }
        catch { }
    }
}
