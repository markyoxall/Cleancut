using CleanCut.Application.DTOs;
using Microsoft.Extensions.Logging;

namespace CleanCut.BlazorWebApp.State;

public class UiStateService : IUiStateService
{
    private readonly ICustomersState _users;
    private readonly IProductsState _products;
    private readonly ICountriesState _countries;
    private readonly ILogger<UiStateService> _logger;

    private bool _localLoading;
    private string? _currentMessage;
    private bool _isSuccess;

    // local selection
    private CustomerInfo? _selectedUser;
    private ProductInfo? _selectedProduct;

    public UiStateService(ICustomersState users, IProductsState products, ICountriesState countries, ILogger<UiStateService> logger)
    {
        _users = users;
        _products = products;
        _countries = countries;
        _logger = logger;

        // subscribe to feature messages to forward to UI
        _users.MessageChanged += OnFeatureMessage;
        _products.MessageChanged += OnFeatureMessage;
        _countries.MessageChanged += OnFeatureMessage;

        // subscribe to state changes to notify consumers
        _users.StateChanged += NotifyStateChanged;
        _products.StateChanged += NotifyStateChanged;
        _countries.StateChanged += NotifyStateChanged;
    }

    public bool IsLoading => _localLoading || _users.IsLoading || _products.IsLoading || _countries.IsLoading;
    public string? CurrentMessage => _currentMessage;
    public bool IsSuccess => _isSuccess;

    public CustomerInfo? SelectedUser => _selectedUser;
    public ProductInfo? SelectedProduct => _selectedProduct;

    public event Action? StateChanged;
    public event Action<string, bool>? MessageChanged;

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
        MessageChanged?.Invoke(message, isSuccess);
        NotifyStateChanged();
    }

    public void ClearMessage()
    {
        _currentMessage = null;
        NotifyStateChanged();
    }

    public void SetSelectedUser(CustomerInfo? user)
    {
        _selectedUser = user;
        NotifyStateChanged();
    }

    public void SetSelectedProduct(ProductInfo? product)
    {
        _selectedProduct = product;
        NotifyStateChanged();
    }

    private void OnFeatureMessage(string message, bool isSuccess)
    {
        // optionally translate or filter here
        _currentMessage = message;
        _isSuccess = isSuccess;
        MessageChanged?.Invoke(message, isSuccess);
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => StateChanged?.Invoke();
}
