using Microsoft.AspNetCore.Components;
using CleanCut.Application.DTOs;
using CleanCut.BlazorWebApp.State;

namespace CleanCut.BlazorWebApp.Components.Base;

/// <summary>
/// Base component class with built-in state management capabilities
/// </summary>
public abstract class StatefulComponent : ComponentBase, IDisposable
{
    [Inject] protected IAppStateService AppState { get; set; } = default!;
    [Inject] protected ILogger<StatefulComponent> Logger { get; set; } = default!;

    // Local component state
    protected bool LocalIsLoading { get; set; }
    protected string? LocalMessage { get; set; }
    protected bool LocalIsSuccess { get; set; }

    // State management helpers
    protected bool IsLoading => AppState.IsLoading || LocalIsLoading;
    protected string? Message => LocalMessage ?? AppState.CurrentMessage;
    protected bool IsSuccess => LocalIsSuccess || AppState.IsSuccess;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        
        // Subscribe to global state changes
        AppState.StateChanged += OnGlobalStateChanged;
        AppState.MessageChanged += OnGlobalMessageChanged;
        
        Logger.LogDebug("Component {ComponentName} initialized with state management", GetType().Name);
    }

    protected virtual void OnGlobalStateChanged()
    {
        InvokeAsync(StateHasChanged);
    }

    protected virtual void OnGlobalMessageChanged(string message)
    {
        InvokeAsync(StateHasChanged);
    }

    // Helper methods for state management
    protected void SetLocalLoading(bool isLoading)
    {
        LocalIsLoading = isLoading;
        StateHasChanged();
    }

    protected void SetLocalMessage(string message, bool isSuccess = true)
    {
        LocalMessage = message;
        LocalIsSuccess = isSuccess;
        StateHasChanged();
    }

    protected void ClearLocalMessage()
    {
        LocalMessage = null;
        StateHasChanged();
    }

    protected async Task ExecuteWithLoading(Func<Task> action, string? successMessage = null, string? errorMessage = null)
    {
        SetLocalLoading(true);
        ClearLocalMessage();

        try
        {
            await action();
            
            if (!string.IsNullOrEmpty(successMessage))
            {
                SetLocalMessage(successMessage, true);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error executing action in component {ComponentName}", GetType().Name);
            
            var message = errorMessage ?? "An error occurred. Please try again.";
            SetLocalMessage(message, false);
        }
        finally
        {
            SetLocalLoading(false);
        }
    }

    protected async Task<T?> ExecuteWithLoading<T>(Func<Task<T>> action, string? successMessage = null, string? errorMessage = null)
    {
        SetLocalLoading(true);
        ClearLocalMessage();

        try
        {
            var result = await action();
            
            if (!string.IsNullOrEmpty(successMessage))
            {
                SetLocalMessage(successMessage, true);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error executing action in component {ComponentName}", GetType().Name);
            
            var message = errorMessage ?? "An error occurred. Please try again.";
            SetLocalMessage(message, false);
            
            return default;
        }
        finally
        {
            SetLocalLoading(false);
        }
    }

    // Virtual methods for derived components to override
    protected virtual Task OnUsersChangedAsync(List<UserDto> users) => Task.CompletedTask;
    protected virtual Task OnProductsChangedAsync(List<ProductDto> products) => Task.CompletedTask;

    public virtual void Dispose()
    {
        // Unsubscribe from global state changes
        if (AppState != null)
        {
            AppState.StateChanged -= OnGlobalStateChanged;
            AppState.MessageChanged -= OnGlobalMessageChanged;
        }
        
        Logger.LogDebug("Component {ComponentName} disposed", GetType().Name);
    }
}