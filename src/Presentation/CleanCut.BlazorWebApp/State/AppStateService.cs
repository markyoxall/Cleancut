using CleanCut.Application.DTOs;
using CleanCut.BlazorWebApp.Services;

namespace CleanCut.BlazorWebApp.State;

/// <summary>
/// Global application state service for managing shared data across components
/// </summary>
public interface IAppStateService
{
    // State properties
    List<UserDto> Users { get; }
    List<ProductDto> Products { get; }
    UserDto? SelectedUser { get; }
    ProductDto? SelectedProduct { get; }
    bool IsLoading { get; }
    string? CurrentMessage { get; }
    bool IsSuccess { get; }

    // State change events
    event Action? StateChanged;
    event Action<string>? MessageChanged;
    event Action<List<UserDto>>? UsersChanged;
    event Action<List<ProductDto>>? ProductsChanged;

    // Actions
    Task LoadUsersAsync();
    Task LoadProductsAsync(Guid? userId = null);
    Task<ProductDto?> CreateProductAsync(CreateProductRequest request);
    Task<ProductDto?> UpdateProductAsync(Guid id, UpdateProductRequest request);
    Task<bool> DeleteProductAsync(Guid id);
    Task<UserDto?> CreateUserAsync(CreateUserRequest request);
    Task<UserDto?> UpdateUserAsync(Guid id, UpdateUserRequest request);
    Task<bool> DeleteUserAsync(Guid id);

    // UI State management
    void SetLoading(bool isLoading);
    void SetMessage(string message, bool isSuccess = true);
    void ClearMessage();
    void SetSelectedUser(UserDto? user);
    void SetSelectedProduct(ProductDto? product);
    void InvalidateCache();
}

public class AppStateService : IAppStateService
{
    private readonly IUserApiService _userApi;
    private readonly IProductApiService _productApi;
    private readonly ILogger<AppStateService> _logger;

    // State fields
    private List<UserDto> _users = new();
    private List<ProductDto> _products = new();
    private UserDto? _selectedUser;
    private ProductDto? _selectedProduct;
    private bool _isLoading;
    private string? _currentMessage;
    private bool _isSuccess;

    // Caching
    private DateTime _usersLastLoaded = DateTime.MinValue;
    private DateTime _productsLastLoaded = DateTime.MinValue;
    private readonly TimeSpan _cacheExpiry = TimeSpan.FromMinutes(5);

    public AppStateService(
        IUserApiService userApi,
        IProductApiService productApi,
        ILogger<AppStateService> logger)
    {
        _userApi = userApi;
        _productApi = productApi;
        _logger = logger;
    }

    // State properties
    public List<UserDto> Users => _users;
    public List<ProductDto> Products => _products;
    public UserDto? SelectedUser => _selectedUser;
    public ProductDto? SelectedProduct => _selectedProduct;
    public bool IsLoading => _isLoading;
    public string? CurrentMessage => _currentMessage;
    public bool IsSuccess => _isSuccess;

    // Events
    public event Action? StateChanged;
    public event Action<string>? MessageChanged;
    public event Action<List<UserDto>>? UsersChanged;
    public event Action<List<ProductDto>>? ProductsChanged;

    // State actions
    public async Task LoadUsersAsync()
    {
        if (DateTime.UtcNow - _usersLastLoaded < _cacheExpiry && _users.Any())
        {
            _logger.LogInformation("Using cached users data");
            return;
        }

        SetLoading(true);
        try
        {
            _logger.LogInformation("Loading users from API");
            _users = await _userApi.GetAllUsersAsync();
            _usersLastLoaded = DateTime.UtcNow;
            
            UsersChanged?.Invoke(_users);
            NotifyStateChanged();
            
            _logger.LogInformation("Loaded {UserCount} users", _users.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading users");
            SetMessage("Failed to load users", false);
        }
        finally
        {
            SetLoading(false);
        }
    }

    public async Task LoadProductsAsync(Guid? userId = null)
    {
        if (userId == null && DateTime.UtcNow - _productsLastLoaded < _cacheExpiry && _products.Any())
        {
            _logger.LogInformation("Using cached products data");
            return;
        }

        SetLoading(true);
        try
        {
            _logger.LogInformation("Loading products from API for user {UserId}", userId);
            
            if (userId.HasValue)
            {
                _products = await _productApi.GetProductsByUserAsync(userId.Value);
            }
            else if (_users.Any())
            {
                // Load products for first user if no specific user selected
                var firstUser = _users.First();
                _products = await _productApi.GetProductsByUserAsync(firstUser.Id);
                _selectedUser = firstUser;
            }
            else
            {
                _products = new List<ProductDto>();
            }
            
            _productsLastLoaded = DateTime.UtcNow;
            
            ProductsChanged?.Invoke(_products);
            NotifyStateChanged();
            
            _logger.LogInformation("Loaded {ProductCount} products", _products.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading products");
            SetMessage("Failed to load products", false);
        }
        finally
        {
            SetLoading(false);
        }
    }

    public async Task<ProductDto?> CreateProductAsync(CreateProductRequest request)
    {
        SetLoading(true);
        try
        {
            _logger.LogInformation("Creating product {ProductName}", request.Name);
            
            var product = await _productApi.CreateProductAsync(request);
            
            // Update local state
            _products.Add(product);
            _selectedProduct = product;
            
            ProductsChanged?.Invoke(_products);
            NotifyStateChanged();
            SetMessage($"Product '{product.Name}' created successfully");
            
            return product;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product");
            SetMessage("Failed to create product", false);
            return null;
        }
        finally
        {
            SetLoading(false);
        }
    }

    public async Task<ProductDto?> UpdateProductAsync(Guid id, UpdateProductRequest request)
    {
        SetLoading(true);
        try
        {
            _logger.LogInformation("Updating product {ProductId}", id);
            
            var product = await _productApi.UpdateProductAsync(id, request);
            
            // Update local state
            var index = _products.FindIndex(p => p.Id == id);
            if (index >= 0)
            {
                _products[index] = product;
            }
            _selectedProduct = product;
            
            ProductsChanged?.Invoke(_products);
            NotifyStateChanged();
            SetMessage($"Product '{product.Name}' updated successfully");
            
            return product;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product");
            SetMessage("Failed to update product", false);
            return null;
        }
        finally
        {
            SetLoading(false);
        }
    }

    public async Task<bool> DeleteProductAsync(Guid id)
    {
        SetLoading(true);
        try
        {
            _logger.LogInformation("Deleting product {ProductId}", id);
            
            var product = _products.FirstOrDefault(p => p.Id == id);
            var success = await _productApi.DeleteProductAsync(id);
            
            if (success)
            {
                // Update local state
                _products.RemoveAll(p => p.Id == id);
                if (_selectedProduct?.Id == id)
                {
                    _selectedProduct = null;
                }
                
                ProductsChanged?.Invoke(_products);
                NotifyStateChanged();
                SetMessage($"Product '{product?.Name}' deleted successfully");
            }
            
            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product");
            SetMessage("Failed to delete product", false);
            return false;
        }
        finally
        {
            SetLoading(false);
        }
    }

    public async Task<UserDto?> CreateUserAsync(CreateUserRequest request)
    {
        SetLoading(true);
        try
        {
            _logger.LogInformation("Creating user {Email}", request.Email);
            
            var user = await _userApi.CreateUserAsync(request);
            
            // Update local state
            _users.Add(user);
            _selectedUser = user;
            
            UsersChanged?.Invoke(_users);
            NotifyStateChanged();
            SetMessage($"User '{user.FirstName} {user.LastName}' created successfully");
            
            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            SetMessage("Failed to create user", false);
            return null;
        }
        finally
        {
            SetLoading(false);
        }
    }

    public async Task<UserDto?> UpdateUserAsync(Guid id, UpdateUserRequest request)
    {
        SetLoading(true);
        try
        {
            _logger.LogInformation("Updating user {UserId}", id);
            
            var user = await _userApi.UpdateUserAsync(id, request);
            
            // Update local state
            var index = _users.FindIndex(u => u.Id == id);
            if (index >= 0)
            {
                _users[index] = user;
            }
            _selectedUser = user;
            
            UsersChanged?.Invoke(_users);
            NotifyStateChanged();
            SetMessage($"User '{user.FirstName} {user.LastName}' updated successfully");
            
            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user");
            SetMessage("Failed to update user", false);
            return null;
        }
        finally
        {
            SetLoading(false);
        }
    }

    public async Task<bool> DeleteUserAsync(Guid id)
    {
        SetLoading(true);
        try
        {
            _logger.LogInformation("Deleting user {UserId}", id);
            
            var user = _users.FirstOrDefault(u => u.Id == id);
            var success = await _userApi.DeleteUserAsync(id);
            
            if (success)
            {
                // Update local state
                _users.RemoveAll(u => u.Id == id);
                if (_selectedUser?.Id == id)
                {
                    _selectedUser = null;
                }
                
                UsersChanged?.Invoke(_users);
                NotifyStateChanged();
                SetMessage($"User '{user?.FirstName} {user?.LastName}' deleted successfully");
            }
            
            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user");
            SetMessage("Failed to delete user", false);
            return false;
        }
        finally
        {
            SetLoading(false);
        }
    }

    // UI State management
    public void SetLoading(bool isLoading)
    {
        if (_isLoading != isLoading)
        {
            _isLoading = isLoading;
            NotifyStateChanged();
        }
    }

    public void SetMessage(string message, bool isSuccess = true)
    {
        _currentMessage = message;
        _isSuccess = isSuccess;
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
        _usersLastLoaded = DateTime.MinValue;
        _productsLastLoaded = DateTime.MinValue;
        _logger.LogInformation("Cache invalidated");
    }

    private void NotifyStateChanged()
    {
        StateChanged?.Invoke();
    }
}