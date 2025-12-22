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

    private static ThreadExceptionEventHandler? _threadExceptionHandler;
    private static UnhandledExceptionEventHandler? _domain_exception_handler;
    private static EventHandler<UnobservedTaskExceptionEventArgs>? _taskExceptionHandler;

    // Module initializer runs as early as possible when the assembly is loaded.
    // Configure a minimal fallback logger here only; heavy fallback handlers are installed from Main
    [ModuleInitializer]
    public static void ModuleInitialize()
    {
        try
        {
            // Attempt to locate the project base path (prefer repository/project root) instead of
            // the binaries folder so early logs are written to a consistent location.
            string basePath = ResolveProjectBasePath() ?? AppContext.BaseDirectory ?? Directory.GetCurrentDirectory();

            var logDirectory = Path.Combine(basePath, "logs");
            _fallbackLogFile = Path.Combine(logDirectory, "cleancut-startup-.log");
            _fallbackCrashFile = Path.Combine(logDirectory, "cleancut-fallback-errors.log");

            try { Directory.CreateDirectory(logDirectory); } catch { }

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

    /// <summary>
    /// Try to resolve a project/repo base path to place logs and other runtime files next to the project
    /// instead of the build output folder. Returns null if resolution fails.
    /// </summary>
    private static string? ResolveProjectBasePath()
    {
        try
        {
            var startDir = new DirectoryInfo(AppContext.BaseDirectory ?? Directory.GetCurrentDirectory());
            DirectoryInfo? dir = startDir;
            while (dir != null)
            {
                // prefer repository root detection via .git or solution file
                if (Directory.Exists(Path.Combine(dir.FullName, ".git")))
                    return dir.FullName;

                var sln = dir.GetFiles("*.sln");
                if (sln.Length > 0)
                    return dir.FullName;

                // fallback: look for this project folder name (CleanCut.WinApp)
                if (string.Equals(dir.Name, "CleanCut.WinApp", StringComparison.OrdinalIgnoreCase))
                    return dir.FullName;

                // look for any csproj to treat as project root
                var csproj = dir.GetFiles("*.csproj");
                if (csproj.Length > 0)
                    return dir.FullName;

                dir = dir.Parent;
            }
        }
        catch
        {
            // swallow; resolution is best-effort only
        }

        return null;
    }

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
            // Use same resolution as module initializer to ensure consistent folder
            var basePath = ResolveProjectBasePath() ?? AppContext.BaseDirectory ?? Directory.GetCurrentDirectory();
            var logDirectory = Path.Combine(basePath, "logs");
            _fallbackLogFile = Path.Combine(logDirectory, "cleancut-startup-.log");
            _fallbackCrashFile = Path.Combine(logDirectory, "cleancut-fallback-errors.log");
            try { Directory.CreateDirectory(logDirectory); } catch { }

            StartupDiagnostics.Install(_fallbackCrashFile);

            // Configure services (this will configure Serilog again to the configured sinks)
            var serviceProvider = ServiceConfiguration.ConfigureServices();

            // Wire global exception handler from DI
            // Guard: if the required service is not registered, log and show an informative message
            CleanCut.Application.Handlers.GlobalException.IGlobalExceptionHandler? globalHandler = null;
            try
            {
                globalHandler = serviceProvider.GetService<CleanCut.Application.Handlers.GlobalException.IGlobalExceptionHandler>();
                if (globalHandler == null)
                {
                    // If this happens, the DI registration is missing; write fatal to Serilog and show MessageBox
                    Log.Fatal("Global exception handler service not registered in DI container");
                    MessageBox.Show("Critical configuration error: global exception handler is not registered. Please check application configuration.", "Configuration Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            catch (Exception ex)
            {
                try { Log.Fatal(ex, "Error resolving IGlobalExceptionHandler from DI"); } catch { }
                MessageBox.Show($"Critical error while initializing application: {ex.Message}", "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Ensure WinForms routes UI exceptions to ThreadException
            System.Windows.Forms.Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

            _threadExceptionHandler = new ThreadExceptionEventHandler((s, e) => _ = globalHandler.HandleAsync(e.Exception, CleanCut.Application.Handlers.GlobalException.ExceptionSource.UIThread));
            System.Windows.Forms.Application.ThreadException += _threadExceptionHandler;

            _domain_exception_handler = new UnhandledExceptionEventHandler((s, e) => {
                if (e.ExceptionObject is Exception ex)
                    _ = globalHandler.HandleAsync(ex, CleanCut.Application.Handlers.GlobalException.ExceptionSource.AppDomain);
                else
                    _ = globalHandler.HandleAsync(new Exception("Non-Exception thrown in AppDomain.UnhandledException"), CleanCut.Application.Handlers.GlobalException.ExceptionSource.AppDomain);
            });
            AppDomain.CurrentDomain.UnhandledException += _domain_exception_handler;

            _taskExceptionHandler = new EventHandler<UnobservedTaskExceptionEventArgs>((s, e) => {
                _ = globalHandler.HandleAsync(e.Exception, CleanCut.Application.Handlers.GlobalException.ExceptionSource.TaskScheduler);
                e.SetObserved();
            });
            TaskScheduler.UnobservedTaskException += _taskExceptionHandler;

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
                if (_threadExceptionHandler != null)
                    System.Windows.Forms.Application.ThreadException -= _threadExceptionHandler;

                if (_domain_exception_handler != null)
                    AppDomain.CurrentDomain.UnhandledException -= _domain_exception_handler;

                if (_taskExceptionHandler != null)
                    TaskScheduler.UnobservedTaskException -= _taskExceptionHandler;
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
                MessageBox.Show($"A fatal error occurred: {ex.Message}", "Fatal Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch { }
        }
        finally
        {
            try { Log.CloseAndFlush(); } catch { }
        }
    }
}
