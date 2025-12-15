using CleanCut.Application.DTOs;
using CleanCut.BlazorWebApp.Services;

namespace CleanCut.BlazorWebApp.State;

/// <summary>
/// Global application state service for managing shared data across components
/// </summary>
public interface IAppStateFacade
{
    // State properties
    List<CustomerInfo> Customers { get; }
    List<ProductInfo> Products { get; }
    CustomerInfo? SelectedCustomer { get; }
    ProductInfo? SelectedProduct { get; }
    bool IsLoading { get; }
    string? CurrentMessage { get; }
    bool IsSuccess { get; }

    // State change events
    event Action? StateChanged;
    event Action<string>? MessageChanged;
    event Action<List<CustomerInfo>>? CustomersChanged;
    event Action<List<ProductInfo>>? ProductsChanged;

    // Actions
    Task LoadAllProductsAsync();
    Task LoadCustomersAsync();
    Task LoadProductsAsync(Guid? customerId = null);
    Task<ProductInfo?> CreateProductAsync(CreateProductRequest request);
    Task<ProductInfo?> UpdateProductAsync(Guid id, UpdateProductRequest request);
    Task<bool> DeleteProductAsync(Guid id);
    Task<CustomerInfo?> CreateCustomerAsync(CreateCustomerRequest request);
    Task<CustomerInfo?> UpdateCustomerAsync(Guid id, UpdateCustomerRequest request);
    Task<bool> DeleteCustomerAsync(Guid id);

    // UI State management
    void SetLoading(bool isLoading);
    void SetMessage(string message, bool isSuccess = true);
    void ClearMessage();
    void SetSelectedCustomer(CustomerInfo? user);
    void SetSelectedProduct(ProductInfo? product);
    void InvalidateCache();
}
