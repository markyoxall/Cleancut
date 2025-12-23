using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CleanCut.WinApp.Infrastructure;

/// <summary>
/// Helper to install and uninstall global exception handlers (UI thread, AppDomain and TaskScheduler)
/// and route exceptions to the application's <c>IGlobalExceptionHandler</c> resolved from DI.
/// </summary>
public sealed class GlobalExceptionHandlers
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger _logger;

    private ThreadExceptionEventHandler? _threadExceptionHandler;
    private UnhandledExceptionEventHandler? _domainExceptionHandler;
    private EventHandler<UnobservedTaskExceptionEventArgs>? _taskExceptionHandler;

    /// <summary>
    /// Creates a new instance that will use the provided <paramref name="serviceProvider"/> to
    /// resolve the global exception handler and logger.
    /// </summary>
    /// <param name="serviceProvider">Service provider to resolve handler and logging services.</param>
    public GlobalExceptionHandlers(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        var factory = serviceProvider.GetService<ILoggerFactory>();
        _logger = factory?.CreateLogger(typeof(GlobalExceptionHandlers).FullName ?? "GlobalExceptionHandlers") ?? Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
    }

    /// <summary>
    /// Install the global handlers. Returns true when installed successfully; false when a required
    /// handler could not be resolved and the caller should abort startup.
    /// </summary>
    public bool Install()
    {
        try
        {
            // Resolve the application global exception handler from DI
            var globalHandler = _serviceProvider.GetService<CleanCut.Application.Handlers.GlobalException.IGlobalExceptionHandler>();
            if (globalHandler == null)
            {
                _logger.LogCritical("Global exception handler service not registered in DI container");
                DeveloperDialog.ShowFatal("Configuration Error", "Critical configuration error: global exception handler is not registered. Please check application configuration.");
                return false;
            }

            // Ensure WinForms routes UI exceptions to ThreadException
            System.Windows.Forms.Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

            _threadExceptionHandler = new System.Threading.ThreadExceptionEventHandler((s, e) => _ = globalHandler.HandleAsync(e.Exception, CleanCut.Application.Handlers.GlobalException.ExceptionSource.UIThread));
            System.Windows.Forms.Application.ThreadException += _threadExceptionHandler;

            _domainExceptionHandler = new UnhandledExceptionEventHandler((s, e) =>
            {
                if (e.ExceptionObject is Exception ex)
                    _ = globalHandler.HandleAsync(ex, CleanCut.Application.Handlers.GlobalException.ExceptionSource.AppDomain);
                else
                    _ = globalHandler.HandleAsync(new Exception("Non-Exception thrown in AppDomain.UnhandledException"), CleanCut.Application.Handlers.GlobalException.ExceptionSource.AppDomain);
            });
            AppDomain.CurrentDomain.UnhandledException += _domainExceptionHandler;

            _taskExceptionHandler = new EventHandler<UnobservedTaskExceptionEventArgs>((s, e) =>
            {
                _ = globalHandler.HandleAsync(e.Exception, CleanCut.Application.Handlers.GlobalException.ExceptionSource.TaskScheduler);
                e.SetObserved();
            });
            TaskScheduler.UnobservedTaskException += _taskExceptionHandler;

            _logger.LogInformation("Global exception handlers installed");
            return true;
        }
        catch (Exception ex)
        {
            try { _logger.LogCritical(ex, "Failed installing global exception handlers"); } catch { }
            try { DeveloperDialog.ShowFatal("Fatal Error", $"Critical error while initializing application: {ex.Message}"); } catch { }
            return false;
        }
    }

    /// <summary>
    /// Uninstall previously installed handlers.
    /// </summary>
    public void Uninstall()
    {
        try
        {
            if (_threadExceptionHandler != null)
            {
                try { System.Windows.Forms.Application.ThreadException -= _threadExceptionHandler; } catch { }
                _threadExceptionHandler = null;
            }

            if (_domainExceptionHandler != null)
            {
                try { AppDomain.CurrentDomain.UnhandledException -= _domainExceptionHandler; } catch { }
                _domainExceptionHandler = null;
            }

            if (_taskExceptionHandler != null)
            {
                try { TaskScheduler.UnobservedTaskException -= _taskExceptionHandler; } catch { }
                _taskExceptionHandler = null;
            }

            _logger.LogInformation("Global exception handlers uninstalled");
        }
        catch (Exception ex)
        {
            try { _logger.LogWarning(ex, "Error while uninstalling global exception handlers"); } catch { }
        }
    }
}
