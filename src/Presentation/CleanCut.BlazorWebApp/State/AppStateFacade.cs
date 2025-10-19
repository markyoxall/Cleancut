using CleanCut.Application.DTOs;
using CleanCut.BlazorWebApp.Services;
using Microsoft.Extensions.Logging;

namespace CleanCut.BlazorWebApp.State;

public class AppStateFacade : IAppStateFacade
{
    private readonly IUsersState _usersState;
    private readonly IProductsState _productsState;
    private readonly ICountriesState _countriesState;
    private readonly ILogger<AppStateFacade> _logger;

    // Local UI/message state (pages may set these)
    private bool _localLoading;
    private string? _currentMessage;
    private bool _isSuccess;

    // Local selection (feature states may expose selection later)
    private UserDto? _selectedUser;
    private ProductDto? _selectedProduct;

    public AppStateFacade(
        IUsersState usersState,
        IProductsState productsState,
        ICountriesState countriesState,
        ILogger<AppStateFacade> logger)
    {
        _usersState = usersState;
        _productsState = productsState;
        _countriesState = countriesState;
        _logger = logger;

        // Forward feature state-changes to facade subscribers
        _usersState.StateChanged += OnFeatureStateChanged;
        _productsState.StateChanged += OnFeatureStateChanged;
        _countriesState.StateChanged += OnFeatureStateChanged;

        // Forward feature messages into facade (feature MessageChanged is (string,bool))
        _usersState.MessageChanged += OnFeatureMessage;
        _productsState.MessageChanged += OnFeatureMessage;
        _countriesState.MessageChanged += OnFeatureMessage;

        // Forward collection events
        _usersState.UsersChanged += users => UsersChanged?.Invoke(new List<UserDto>(users));
        _productsState.ProductsChanged += products => ProductsChanged?.Invoke(new List<ProductDto>(products));
    }

    // Composed loading: any feature loading OR local loading set by pages
    public bool IsLoading => _localLoading || _usersState.IsLoading || _productsState.IsLoading || _countriesState.IsLoading;

    // Legacy surface (kept for compatibility)
    public List<UserDto> Users => _usersState.Users?.ToList() ?? new List<UserDto>();
    public List<ProductDto> Products => _productsState.Products?.ToList() ?? new List<ProductDto>();
    public UserDto? SelectedUser => _selectedUser;
    public ProductDto? SelectedProduct => _selectedProduct;
    public string? CurrentMessage => _currentMessage;
    public bool IsSuccess => _isSuccess;

    // Events required by IAppStateService
    public event Action? StateChanged;
    public event Action<string>? MessageChanged;
    public event Action<List<UserDto>>? UsersChanged;
    public event Action<List<ProductDto>>? ProductsChanged;

    // Delegated load/write operations
    public async Task LoadAllProductsAsync() => await _productsState.LoadAllAsync(force: false);
    public async Task LoadUsersAsync() => await _usersState.LoadAsync(force: false);
    public async Task LoadProductsAsync(Guid? userId = null)
    {
        if (userId.HasValue)
            await _productsState.LoadByUserAsync(userId.Value, force: false);
        else
            await _productsState.LoadAllAsync(force: false);
    }

    public async Task<ProductDto?> CreateProductAsync(CreateProductRequest request) => await _productsState.CreateAsync(request);
    public async Task<ProductDto?> UpdateProductAsync(Guid id, UpdateProductRequest request) => await _productsState.UpdateAsync(id, request);
    public async Task<bool> DeleteProductAsync(Guid id) => await _productsState.DeleteAsync(id);

    public async Task<UserDto?> CreateUserAsync(CreateUserRequest request) => await _usersState.CreateAsync(request);
    public async Task<UserDto?> UpdateUserAsync(Guid id, UpdateUserRequest request) => await _usersState.UpdateAsync(id, request);
    public async Task<bool> DeleteUserAsync(Guid id) => await _usersState.DeleteAsync(id);

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

    public void SetSelectedUser(UserDto? user)
    {
        _selectedUser = user;
        NotifyStateChanged();
    }

    public void SetSelectedProduct(ProductDto? product)
    {
        _selectedProduct = product;
        NotifyStateChanged();
    }

    public void InvalidateCache()
    {
        _usersState.Invalidate();
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