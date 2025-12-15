using CleanCut.Application.DTOs;
using CleanCut.BlazorWebApp.Services;
using Microsoft.Extensions.Logging;

namespace CleanCut.BlazorWebApp.State;

public class AppStateFacade : IAppStateFacade
{
    private readonly ICustomersState _customersState;
    private readonly IProductsState _productsState;
    private readonly ICountriesState _countriesState;
    private readonly ILogger<AppStateFacade> _logger;

    // Local UI/message state (pages may set these)
    private bool _localLoading;
    private string? _currentMessage;
    private bool _isSuccess;

    // Local selection (feature states may expose selection later)
    private CustomerInfo? _selectedCustomer;
    private ProductInfo? _selectedProduct;

    public AppStateFacade(
        ICustomersState customersState,
        IProductsState productsState,
        ICountriesState countriesState,
        ILogger<AppStateFacade> logger)
    {
        _customersState = customersState;
        _productsState = productsState;
        _countriesState = countriesState;
        _logger = logger;

        // Forward feature state-changes to facade subscribers
        _customersState.StateChanged += OnFeatureStateChanged;
        _productsState.StateChanged += OnFeatureStateChanged;
        _countriesState.StateChanged += OnFeatureStateChanged;

        // Forward feature messages into facade (feature MessageChanged is (string,bool))
        _customersState.MessageChanged += OnFeatureMessage;
        _productsState.MessageChanged += OnFeatureMessage;
        _countriesState.MessageChanged += OnFeatureMessage;

        // Forward collection events
        _customersState.CustomersChanged += customers => CustomersChanged?.Invoke(new List<CustomerInfo>(customers));
        _productsState.ProductsChanged += products => ProductsChanged?.Invoke(new List<ProductInfo>(products));
    }

    // Composed loading: any feature loading OR local loading set by pages
    public bool IsLoading => _localLoading || _customersState.IsLoading || _productsState.IsLoading || _countriesState.IsLoading;

    // Legacy surface (kept for compatibility)
    public List<CustomerInfo> Customers => _customersState.Customers?.ToList() ?? new List<CustomerInfo>();
    public List<ProductInfo> Products => _productsState.Products?.ToList() ?? new List<ProductInfo>();
    public CustomerInfo? SelectedCustomer => _selectedCustomer;
    public ProductInfo? SelectedProduct => _selectedProduct;
    public string? CurrentMessage => _currentMessage;
    public bool IsSuccess => _isSuccess;

    // Events required by IAppStateService
    public event Action? StateChanged;
    public event Action<string>? MessageChanged;
    public event Action<List<CustomerInfo>>? CustomersChanged;
    public event Action<List<ProductInfo>>? ProductsChanged;

    // Delegated load/write operations
    public async Task LoadAllProductsAsync() => await _productsState.LoadAllAsync(force: false);
    public async Task LoadCustomersAsync() => await _customersState.LoadAsync(force: false);
    public async Task LoadProductsAsync(Guid? userId = null)
    {
        if (userId.HasValue)
            await _productsState.LoadByCustomerAsync(userId.Value, force: false);
        else
            await _productsState.LoadAllAsync(force: false);
    }

    public async Task<ProductInfo?> CreateProductAsync(CreateProductRequest request) => await _productsState.CreateAsync(request);
    public async Task<ProductInfo?> UpdateProductAsync(Guid id, UpdateProductRequest request) => await _productsState.UpdateAsync(id, request);
    public async Task<bool> DeleteProductAsync(Guid id) => await _productsState.DeleteAsync(id);

    public async Task<CustomerInfo?> CreateCustomerAsync(CreateCustomerRequest request) => await _customersState.CreateAsync(request);
    public async Task<CustomerInfo?> UpdateCustomerAsync(Guid id, UpdateCustomerRequest request) => await _customersState.UpdateAsync(id, request);
    public async Task<bool> DeleteCustomerAsync(Guid id) => await _customersState.DeleteAsync(id);

    // UI helpers
    public void SetLoading(bool isLoading)
    {
        if (_localLoading == isLoading) return;
        _localLoading = isLoading;
        NotifyStateChanged();
    }

    public void SetMessage(string message, bool isSuccess = true)
    {
        _currentMessage = message;
        _isSuccess = isSuccess;
        // keep legacy single-string MessageChanged for pages that rely on it
        MessageChanged?.Invoke(message);
        NotifyStateChanged();
    }

    public void ClearMessage()
    {
        _currentMessage = null;
        NotifyStateChanged();
    }

    public void SetSelectedCustomer(CustomerInfo? customer)
    {
        _selectedCustomer = customer;
        NotifyStateChanged();
    }

    public void SetSelectedProduct(ProductInfo? product)
    {
        _selectedProduct = product;
        NotifyStateChanged();
    }

    public void InvalidateCache()
    {
        _customersState.Invalidate();
        _productsState.Invalidate();
        _countriesState.Invalidate();
        _logger.LogInformation("AppStateFacade: invalidated feature caches");
        NotifyStateChanged();
    }

    // Feature message handler (feature states send (message, isSuccess) — adapt to legacy MessageChanged)
    private void OnFeatureMessage(string message, bool isSuccess)
    {
        _currentMessage = message;
        _isSuccess = isSuccess;
        MessageChanged?.Invoke(message);
        NotifyStateChanged();
    }

    private void OnFeatureStateChanged() => NotifyStateChanged();

    private void NotifyStateChanged() => StateChanged?.Invoke();
}
