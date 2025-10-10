using CleanCut.Application.DTOs;

namespace CleanCut.WinApp.Views.Users;

/// <summary>
/// Interface for User List View
/// </summary>
public interface IUserListView : MVP.IView
{
    /// <summary>
    /// Event raised when user wants to add a new user
    /// </summary>
    event EventHandler? AddUserRequested;
    
    /// <summary>
    /// Event raised when user wants to edit a user
    /// </summary>
    event EventHandler<Guid>? EditUserRequested;
    
    /// <summary>
    /// Event raised when user wants to delete a user
    /// </summary>
    event EventHandler<Guid>? DeleteUserRequested;
    
    /// <summary>
    /// Event raised when user wants to refresh the list
    /// </summary>
    event EventHandler? RefreshRequested;
    
    /// <summary>
    /// Display the list of users
    /// </summary>
    void DisplayUsers(IEnumerable<UserDto> users);
    
    /// <summary>
    /// Clear the users list
    /// </summary>
    void ClearUsers();
    
    /// <summary>
    /// Get the selected user ID
    /// </summary>
    Guid? GetSelectedUserId();
}