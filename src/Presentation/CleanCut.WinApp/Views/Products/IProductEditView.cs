using CleanCut.Application.DTOs;

namespace CleanCut.WinApp.Views.Products;

/// <summary>
/// Interface for Product Edit/Create View
/// </summary>
public interface IProductEditView : MVP.IView
{
    /// <summary>
    /// Event raised when user wants to save
    /// </summary>
    event EventHandler? SaveRequested;
    
    /// <summary>
    /// Event raised when user wants to cancel
    /// </summary>
    event EventHandler? CancelRequested;
    
    /// <summary>
    /// Get the product data from the form
    /// </summary>
    ProductEditModel GetProductData();
    
    /// <summary>
    /// Set the product data in the form
    /// </summary>
    void SetProductData(ProductEditModel product);
    
    /// <summary>
    /// Validate the form and return validation errors
    /// </summary>
    Dictionary<string, string> ValidateForm();
    
    /// <summary>
    /// Clear all form fields
    /// </summary>
    void ClearForm();
    
    /// <summary>
    /// Set the available users for selection
    /// </summary>
    void SetAvailableUsers(IEnumerable<CustomerInfo> users);
    
    /// <summary>
    /// Set the available customers for selection (alias for SetAvailableUsers to match updated terminology)
    /// </summary>
    void SetAvailableCustomers(IEnumerable<CustomerInfo> customers);
}

/// <summary>
/// Model for product edit form
/// </summary>
public class ProductEditModel
{
    public Guid? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool IsAvailable { get; set; } = true;
    public Guid UserId { get; set; }
}