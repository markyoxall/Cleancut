using CleanCut.WinApp.Services.Management;
using System.Threading.Tasks;

namespace CleanCut.WinApp.MVP;

/// <summary>
/// Base class for all presenters in the MVP pattern.
/// 
/// SOLID mapping:
/// - SRP: provides common presenter concerns (initialization, error handling) separated from view-specific logic.
/// - OCP: virtual methods allow derived presenters to extend behavior without modifying base class.
/// - LSP: derived presenters should be substitutable for the base presenter (no stronger preconditions).
/// - DIP: depends on abstractions for preferences and other services used via constructor injection in derived classes.
/// </summary>
/// <typeparam name="TView">The type of the view interface</typeparam>
public abstract class BasePresenter<TView> : IDisposable where TView : IView
{
    /// <summary>
    /// The view instance this presenter controls.
    /// </summary>
    protected TView View { get; }

    /// <summary>
    /// Optional user preferences loaded by the loader. Protected so derived presenters may read them.
    /// </summary>
    protected UserPreferences? Preferences { get; }

    protected BasePresenter(TView view)
    {
        View = view ?? throw new ArgumentNullException(nameof(view));
    }

    // New constructor to accept preferences
    protected BasePresenter(TView view, UserPreferences? preferences)
    {
        View = view ?? throw new ArgumentNullException(nameof(view));
        Preferences = preferences;
    }

    /// <summary>
    /// Initialize the presenter. Override in derived classes for specific initialization.
    /// </summary>
    public virtual void Initialize()
    {
        // Example: Use preferences if available
        if (Preferences != null)
        {
            // Apply layout, theme, etc. (derived classes implement specifics)
        }
    }

    /// <summary>
    /// Cleanup resources when the presenter is disposed.
    /// </summary>
    public virtual void Cleanup()
    {
    }

    /// <summary>
    /// Handle errors in a consistent way across presenters.
    /// </summary>
    protected virtual void HandleError(Exception ex)
    {
        if (View is Control control && control.InvokeRequired)
        {
            control.Invoke(() => View.ShowError($"An error occurred: {ex.Message}"));
            return;
        }

        View.ShowError($"An error occurred: {ex.Message}");
    }

    /// <summary>
    /// Execute an async operation with error handling and loading state.
    /// </summary>
    protected async Task ExecuteAsync(Func<Task> operation)
    {
        try
        {
            if (View is Control control && control.InvokeRequired)
            {
                control.Invoke(() => View.SetLoading(true));
            }
            else
            {
                View.SetLoading(true);
            }

            await operation();
        }
        catch (Exception ex)
        {
            HandleError(ex);
        }
        finally
        {
            if (View is Control control && control.InvokeRequired)
            {
                control.Invoke(() => View.SetLoading(false));
            }
            else
            {
                View.SetLoading(false);
            }
        }
    }

    /// <summary>
    /// Execute an async operation with return value, error handling and loading state.
    /// </summary>
    protected async Task<T?> ExecuteAsync<T>(Func<Task<T>> operation)
    {
        try
        {
            if (View is Control control && control.InvokeRequired)
            {
                control.Invoke(() => View.SetLoading(true));
            }
            else
            {
                View.SetLoading(true);
            }

            return await operation();
        }
        catch (Exception ex)
        {
            HandleError(ex);
            return default;
        }
        finally
        {
            if (View is Control control && control.InvokeRequired)
            {
                control.Invoke(() => View.SetLoading(false));
            }
            else
            {
                View.SetLoading(false);
            }
        }
    }

    /// <summary>
    /// Execute an operation quickly without loading indicators (for fast UI operations).
    /// </summary>
    protected void ExecuteQuick(Action operation)
    {
        try
        {
            operation();
        }
        catch (Exception ex)
        {
            HandleError(ex);
        }
    }

    public void Dispose()
    {
        try
        {
            Cleanup();
        }
        catch
        {
            // swallow exceptions during dispose
        }
    }
}
