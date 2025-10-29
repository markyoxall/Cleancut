using Microsoft.AspNetCore.Components;
using CleanCut.Application.DTOs;
using CleanCut.BlazorWebApp.State;

namespace CleanCut.BlazorWebApp.Components.Base;

/// <summary>
/// Base component class with built-in state management capabilities (feature-state driven)
/// </summary>
public abstract class StatefulComponent : ComponentBase, IDisposable
{
    [Inject] protected ICustomersState CustomersState { get; set; } = default!;
    [Inject] protected IProductsState ProductsState { get; set; } = default!;
    [Inject] protected ICountriesState CountriesState { get; set; } = default!;
    [Inject] protected IUiStateService UiState { get; set; } = default!;
    [Inject] protected ILogger<StatefulComponent> Logger { get; set; } = default!;

    // Local component state
    protected bool LocalIsLoading { get; set; }
    protected string? LocalMessage { get; set; }
    protected bool LocalIsSuccess { get; set; }

    // State management helpers
    // Prefer UiState.IsLoading if available (it aggregates feature-state loading)
    protected bool IsLoading => (UiState != null) ? (UiState.IsLoading || LocalIsLoading) : (CustomersState.IsLoading || ProductsState.IsLoading || CountriesState.IsLoading || LocalIsLoading);
    // Prefer global UI message, fall back to local
    protected string? Message => UiState?.CurrentMessage ?? LocalMessage;
    protected bool IsSuccess => UiState?.IsSuccess ?? LocalIsSuccess;

    protected override void OnInitialized()
    {
        base.OnInitialized();

        // Feature state subscriptions (data/caching)
        CustomersState.StateChanged += OnGlobalStateChanged;
        ProductsState.StateChanged += OnGlobalStateChanged;
        CountriesState.StateChanged += OnGlobalStateChanged;

        CustomersState.CustomersChanged += OnCustomersChanged;
        ProductsState.ProductsChanged += OnProductsChanged;
        CountriesState.CountriesChanged += OnCountriesChanged;

        // Feature-level messages -> local message handling
        CustomersState.MessageChanged += OnFeatureMessage;
        ProductsState.MessageChanged += OnFeatureMessage;
        CountriesState.MessageChanged += OnFeatureMessage;

        // Subscribe to new UI state service for global UI events
        if (UiState != null)
        {
            UiState.StateChanged += OnGlobalStateChanged;
            UiState.MessageChanged += OnUiMessage;
        }

        Logger.LogDebug("Component {ComponentName} initialized with feature-state management", GetType().Name);
    }

    protected virtual void OnGlobalStateChanged()
    {
        InvokeAsync(StateHasChanged);
    }

    private void OnUiMessage(string message, bool isSuccess)
    {
        // Let UiState own the canonical message; also mirror to local if desired
        LocalMessage = message;
        LocalIsSuccess = isSuccess;
        InvokeAsync(StateHasChanged);
    }

    private void OnFeatureMessage(string message, bool isSuccess)
    {
        // Mirror feature messages into local UI so pages still show them
        LocalMessage = message;
        LocalIsSuccess = isSuccess;
        InvokeAsync(StateHasChanged);
    }

    // Virtual hooks for derived components
    protected virtual Task OnCustomersChangedAsync(List<CustomerInfo> users) => Task.CompletedTask;
    protected virtual Task OnProductsChangedAsync(List<ProductInfo> products) => Task.CompletedTask;
    protected virtual Task OnCountriesChangedAsync(List<CountryInfo> countries) => Task.CompletedTask;

    private void OnCustomersChanged(List<CustomerInfo> users)
    {
        _ = OnCustomersChangedAsync(users);
        InvokeAsync(StateHasChanged);
    }

    private void OnProductsChanged(List<ProductInfo> products)
    {
        _ = OnProductsChangedAsync(products);
        InvokeAsync(StateHasChanged);
    }

    private void OnCountriesChanged(List<CountryInfo> countries)
    {
        _ = OnCountriesChangedAsync(countries);
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

    public virtual void Dispose()
    {
        // Unsubscribe from feature state events
        CustomersState.StateChanged -= OnGlobalStateChanged;
        ProductsState.StateChanged -= OnGlobalStateChanged;
        CountriesState.StateChanged -= OnGlobalStateChanged;

        CustomersState.CustomersChanged -= OnCustomersChanged;
        ProductsState.ProductsChanged -= OnProductsChanged;
        CountriesState.CountriesChanged -= OnCountriesChanged;

        CustomersState.MessageChanged -= OnFeatureMessage;
        ProductsState.MessageChanged -= OnFeatureMessage;
        CountriesState.MessageChanged -= OnFeatureMessage;

        // Unsubscribe from UI service
        if (UiState != null)
        {
            UiState.StateChanged -= OnGlobalStateChanged;
            UiState.MessageChanged -= OnUiMessage;
        }

        Logger.LogDebug("Component {ComponentName} disposed", GetType().Name);
    }
}