using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using CleanCut.WinApp.Infrastructure;
using System.Runtime.CompilerServices;
using Serilog;


namespace CleanCut.WinApp;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    private static string? _fallbackLogFile;
    private static string? _fallbackCrashFile;

    // Module initializer runs as early as possible when the assembly is loaded.
    // Use it to attach exception handlers and configure a minimal fallback logger
    // before any other code runs (this helps capture failures that occur before
    // Main is entered).
    [ModuleInitializer]
    public static void ModuleInitialize()
    {
        try
        {
            var basePath = AppContext.BaseDirectory ?? Directory.GetCurrentDirectory();
            var logDirectory = Path.Combine(basePath, "logs");
            _fallbackLogFile = Path.Combine(logDirectory, "cleancut-startup-.log");
            _fallbackCrashFile = Path.Combine(logDirectory, "cleancut-fallback-errors.log");

            try { Directory.CreateDirectory(logDirectory); } catch { }

            try
            {
                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Debug()
                    .WriteTo.Console()
                    .WriteTo.File(_fallbackLogFile, rollingInterval: RollingInterval.Day)
                    .CreateLogger();
            }
            catch { }

            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                try { Log.Fatal(e.ExceptionObject as Exception, "Unhandled exception (AppDomain)"); } catch { }
                try { File.AppendAllText(_fallbackCrashFile!, $"UNHANDLED {DateTime.UtcNow:o}\n{e.ExceptionObject}\n\n"); } catch { }
                try { Log.CloseAndFlush(); } catch { }
            };

            AppDomain.CurrentDomain.FirstChanceException += (s, e) =>
            {
                try { File.AppendAllText(_fallbackCrashFile!, $"FIRST_CHANCE {DateTime.UtcNow:o} {e.Exception}\n\n"); } catch { }
            };

            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                try { Log.Fatal(e.Exception, "Unobserved task exception"); } catch { }
                try { File.AppendAllText(_fallbackCrashFile!, $"UNOBSERVED_TASK {DateTime.UtcNow:o}\n{e.Exception}\n\n"); } catch { }
                try { Log.CloseAndFlush(); } catch { }
            };

            System.Windows.Forms.Application.ThreadException += (s, e) =>
            {
                try { Log.Fatal(e.Exception, "Windows Forms thread exception"); } catch { }
                try { File.AppendAllText(_fallbackCrashFile!, $"THREAD_EXCEPTION {DateTime.UtcNow:o}\n{e.Exception}\n\n"); } catch { }
                try { Log.CloseAndFlush(); } catch { }
            };
        }
        catch { }
    }

    [STAThread]
    static async Task Main()
    {

        try
        {
            // To customize application configuration such as set high DPI settings or default font,
            // perform explicit initialization steps to avoid relying on the newer
            // `ApplicationConfiguration` helper which may not be present in all builds.
            System.Windows.Forms.Application.SetHighDpiMode(System.Windows.Forms.HighDpiMode.SystemAware);
            System.Windows.Forms.Application.EnableVisualStyles();
            System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);

            // Configure services (this will configure Serilog again to the configured sinks)
            var serviceProvider = ServiceConfiguration.ConfigureServices();

            // Get logger from DI to produce structured startup messages
            var loggerFactory = serviceProvider.GetRequiredService<Microsoft.Extensions.Logging.ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("CleanCut.WinApp.Program");
            logger.LogInformation("Starting CleanCut WinApp application");
            logger.LogInformation("Working directory: {cwd}", Directory.GetCurrentDirectory());

            // Create and run main form
            var mainForm = serviceProvider.GetRequiredService<MainForm>();
            System.Windows.Forms.Application.Run(mainForm);

            logger.LogInformation("CleanCut WinApp application stopped");
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
