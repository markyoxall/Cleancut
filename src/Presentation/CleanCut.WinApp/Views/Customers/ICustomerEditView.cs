using CleanCut.Application.DTOs;

namespace CleanCut.WinApp.Views.Customers;

/// <summary>
/// Interface for Customer Edit/Create View
/// </summary>
public interface ICustomerEditView : MVP.IView
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
    /// Get the user data from the form
    /// </summary>
    CustomerEditModel GetCustomerData();
    
    /// <summary>
    /// Set the user data in the form
    /// </summary>
    void SetCustomerData(CustomerEditModel user);
    
    /// <summary>
    /// Validate the form and return validation errors
    /// </summary>
    Dictionary<string, string> ValidateForm();
    
    /// <summary>
    /// Clear all form fields
    /// </summary>
    void ClearForm();
}

/// <summary>
/// Model for user edit form
/// </summary>
public class CustomerEditModel
{
    public Guid? Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}