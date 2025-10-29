using CleanCut.Application.DTOs;

namespace CleanCut.WinApp.Views.Products;

/// <summary>
/// Interface for Product List View
/// </summary>
public interface IProductListView : MVP.IView
{
    /// <summary>
    /// Event raised when user wants to add a new product
    /// </summary>
    event EventHandler? AddProductRequested;
    
    /// <summary>
    /// Event raised when user wants to edit a product
    /// </summary>
    event EventHandler<Guid>? EditProductRequested;
    
    /// <summary>
    /// Event raised when user wants to delete a product
    /// </summary>
    event EventHandler<Guid>? DeleteProductRequested;
    
    /// <summary>
    /// Event raised when user wants to refresh the list
    /// </summary>
    event EventHandler? RefreshRequested;
    
    /// <summary>
    /// Event raised when user wants to view products by customer
    /// </summary>
    event EventHandler<Guid>? ViewProductsByCustomerRequested;
    
    /// <summary>
    /// Display the list of products
    /// </summary>
    void DisplayProducts(IEnumerable<ProductInfo> products);
    
    /// <summary>
    /// Clear the products list
    /// </summary>
    void ClearProducts();
    
    /// <summary>
    /// Get the selected product ID
    /// </summary>
    Guid? GetSelectedProductId();
    
    /// <summary>
    /// Set the available customers for filtering
    /// </summary>
    void SetAvailableCustomers(IEnumerable<CustomerInfo> customers);
}