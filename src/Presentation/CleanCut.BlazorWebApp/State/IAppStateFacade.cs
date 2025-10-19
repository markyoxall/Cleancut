using CleanCut.Application.DTOs;
using CleanCut.BlazorWebApp.Services;

namespace CleanCut.BlazorWebApp.State;

/// <summary>
/// Global application state service for managing shared data across components
/// </summary>
public interface IAppStateFacade
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
    Task LoadAllProductsAsync();
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
