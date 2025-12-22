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
    CustomerEditViewModel GetCustomerData();
    
    /// <summary>
    /// Set the user data in the form
    /// </summary>
    void SetCustomerData(CustomerEditViewModel user);
    
    /// <summary>
    /// Validate the form and return validation errors
    /// </summary>
    Dictionary<string, string> ValidateForm();
    
    /// <summary>
    /// Clear all form fields
    /// </summary>
    void ClearForm();

    /// <summary>
    /// Show validation errors mapping field -> message
    /// </summary>
    void ShowValidationErrors(Dictionary<string, string> errors);

    /// <summary>
    /// Clear previously shown validation errors
    /// </summary>
    void ClearValidationErrors();
}

// Model types have been moved to their own files (CustomerEditModel in CustomerEditModel.cs)
