namespace CleanCut.WinApp.MVP;

/// <summary>
/// Base class for all presenters in MVP pattern
/// </summary>
/// <typeparam name="TView">The view interface</typeparam>
public abstract class BasePresenter<TView> where TView : IView
{
    protected TView View { get; }

    protected BasePresenter(TView view)
    {
        View = view ?? throw new ArgumentNullException(nameof(view));
    }

    /// <summary>
    /// Initialize the presenter
    /// </summary>
    public virtual void Initialize()
    {
        // Override in derived classes for initialization logic
    }

    /// <summary>
    /// Cleanup resources when the presenter is disposed
    /// </summary>
    public virtual void Cleanup()
    {
        // Override in derived classes for cleanup logic
    }

    /// <summary>
    /// Handle errors in a consistent way across all presenters
    /// </summary>
    protected virtual void HandleError(Exception ex)
    {
        // Log the error (you can inject ILogger here)
        Console.WriteLine($"Error: {ex.Message}");
        
        // ?? Show error message on UI thread
        if (View is Control control && control.InvokeRequired)
        {
            control.Invoke(() => View.ShowError($"An error occurred: {ex.Message}"));
        }
        else
        {
            View.ShowError($"An error occurred: {ex.Message}");
        }
    }

    /// <summary>
    /// Execute an async operation with error handling and loading state
    /// </summary>
    protected async Task ExecuteAsync(Func<Task> operation)
    {
        try
        {
            // ?? Set loading state on UI thread
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
            // ?? Clear loading state on UI thread
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
    /// Execute an async operation with return value, error handling and loading state
    /// </summary>
    protected async Task<T?> ExecuteAsync<T>(Func<Task<T>> operation)
    {
        try
        {
            // ?? Set loading state on UI thread
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
            // ?? Clear loading state on UI thread
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
    /// ?? Execute an operation quickly without loading indicators (for fast UI operations)
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
}