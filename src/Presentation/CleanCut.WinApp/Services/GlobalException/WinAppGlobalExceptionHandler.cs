using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;

namespace CleanCut.Application.Handlers.GlobalException
{
    /// <summary>
    /// Defines the source of an exception for context when handling.
    /// </summary>
    public enum ExceptionSource
    {
        UIThread,
        AppDomain,
        TaskScheduler,
        Other
    }

    /// <summary>
    /// Abstraction for a global exception handler used by the WinApp host to route
    /// unhandled exceptions into a central place for logging or reporting.
    /// </summary>
    public interface IGlobalExceptionHandler
    {
        /// <summary>
        /// Handle an unhandled exception originating from the specified source.
        /// </summary>
        Task HandleAsync(Exception exception, ExceptionSource source);
    }

    /// <summary>
    /// A simple WinForms-friendly global exception handler implementation that logs
    /// the exception and shows a message box for UI-thread exceptions. This is used
    /// as a minimal host-side handler for the desktop app when no application-wide
    /// handler is registered in other layers.
    /// </summary>
    public class WinAppGlobalExceptionHandler : IGlobalExceptionHandler
    {
        private readonly ILogger<WinAppGlobalExceptionHandler> _logger;

        public WinAppGlobalExceptionHandler(ILogger<WinAppGlobalExceptionHandler> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task HandleAsync(Exception exception, ExceptionSource source)
        {
            try
            {
                _logger.LogError(exception, "Unhandled exception (source={Source})", source);

                // For UI thread exceptions show a friendly dialog so users see the error.
                if (source == ExceptionSource.UIThread)
                {
                    try
                    {
                        MessageBox.Show($"An unexpected error occurred:\n{exception.Message}", "Application Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    catch
                    {
                        // ignore any failures showing a dialog
                    }
                }
            }
            catch
            {
                // Swallow to avoid recursive failures
            }

            return Task.CompletedTask;
        }
    }
}
