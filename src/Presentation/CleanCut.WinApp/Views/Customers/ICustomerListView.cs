using CleanCut.Application.DTOs;

namespace CleanCut.WinApp.Views.Customers;

/// <summary>
/// Interface for Customer List View
/// </summary>
public interface ICustomerListView : MVP.IView
{
    /// <summary>
    /// Event raised when user wants to add a new user
    /// </summary>
    event EventHandler? AddCustomerRequested;
    
    /// <summary>
    /// Event raised when user wants to edit a user
    /// </summary>
    event EventHandler<Guid>? EditCustomerRequested;
    
    /// <summary>
    /// Event raised when user wants to delete a user
    /// </summary>
    event EventHandler<Guid>? DeleteCustomerRequested;
    
    /// <summary>
    /// Event raised when user wants to refresh the list
    /// </summary>
    event EventHandler? RefreshRequested;
    
    /// <summary>
    /// Display the list of users
    /// </summary>
    void DisplayCustomers(IEnumerable<CustomerInfo> users);
    
    /// <summary>
    /// Clear the users list
    /// </summary>
    void ClearCustomers();
    
    /// <summary>
    /// Get the selected user ID
    /// </summary>
    Guid? GetSelectedCustomerId();

    // Apply saved column order and widths
    void ApplyGridPreferences(List<string>? columnOrder, Dictionary<string, int>? columnWidths);
}
