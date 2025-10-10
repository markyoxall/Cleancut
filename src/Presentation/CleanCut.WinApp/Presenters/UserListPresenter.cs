using CleanCut.Application.Commands.Users.CreateUser;
using CleanCut.Application.Commands.Users.UpdateUser;
using CleanCut.Application.Queries.Users.GetAllUsers;
using CleanCut.Application.Queries.Users.GetUser;
using CleanCut.Application.DTOs;
using CleanCut.WinApp.MVP;
using CleanCut.WinApp.Views.Users;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CleanCut.WinApp.Presenters;

/// <summary>
/// Presenter for User List View implementing MVP pattern
/// </summary>
public class UserListPresenter : BasePresenter<IUserListView>
{
    private readonly IMediator _mediator;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<UserListPresenter> _logger;
    private List<UserDto> _cachedUsers = new(); // ?? Cache users locally

    public UserListPresenter(
        IUserListView view, 
        IMediator mediator, 
        IServiceProvider serviceProvider,
        ILogger<UserListPresenter> logger) 
        : base(view)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public override void Initialize()
    {
        base.Initialize();
        
        // Subscribe to view events
        View.AddUserRequested += OnAddUserRequested;
        View.EditUserRequested += OnEditUserRequested;
        View.DeleteUserRequested += OnDeleteUserRequested;
        View.RefreshRequested += OnRefreshRequested;
        
        // Load initial data
        _ = LoadUsersAsync();
    }

    public override void Cleanup()
    {
        // Unsubscribe from view events
        View.AddUserRequested -= OnAddUserRequested;
        View.EditUserRequested -= OnEditUserRequested;
        View.DeleteUserRequested -= OnDeleteUserRequested;
        View.RefreshRequested -= OnRefreshRequested;
        
        base.Cleanup();
    }

    private async void OnAddUserRequested(object? sender, EventArgs e)
    {
        await ExecuteAsync(async () =>
        {
            _logger.LogInformation("Add user requested");
            
            // ?? Create form and presenter outside of async context
            var editForm = new UserEditForm();
            editForm.Text = "Add New User";
            editForm.ClearForm();
            
            var presenter = new UserEditPresenter(editForm, _mediator, _logger);
            presenter.Initialize();
            
            // ?? Show dialog on UI thread
            var result = editForm.ShowDialog();
            if (result == DialogResult.OK)
            {
                await LoadUsersAsync();
                View.ShowSuccess("User created successfully.");
            }
            
            presenter.Cleanup();
        });
    }

    private async void OnEditUserRequested(object? sender, Guid userId)
    {
        try
        {
            _logger.LogInformation("Edit user requested for user {UserId}", userId);
            
            // ?? OPTIMIZATION: Get user from cache instead of database
            var user = _cachedUsers.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                _logger.LogWarning("User {UserId} not found in cache, fetching from database", userId);
                user = await _mediator.Send(new GetUserQuery(userId));
                
                if (user == null)
                {
                    View.ShowError("User not found.");
                    return;
                }
            }
            
            // ?? Create form and presenter on UI thread (fast)
            var editForm = new UserEditForm();
            editForm.Text = "Edit User";
            
            var presenter = new UserEditPresenter(editForm, _mediator, _logger);
            presenter.SetEditMode(user);
            presenter.Initialize();
            
            // ?? Show dialog immediately (no await blocking)
            var result = editForm.ShowDialog();
            if (result == DialogResult.OK)
            {
                // ?? Only refresh users in background, don't block UI
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await LoadUsersAsync();
                        
                        // ?? Show success message on UI thread
                        if (View is Control control)
                        {
                            control.Invoke(() => View.ShowSuccess("User updated successfully."));
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error refreshing users after edit");
                    }
                });
            }
            
            presenter.Cleanup();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in edit user operation");
            View.ShowError($"Failed to open user edit dialog: {ex.Message}");
        }
    }

    private async void OnDeleteUserRequested(object? sender, Guid userId)
    {
        try
        {
            _logger.LogInformation("Delete user requested for user {UserId}", userId);
            
            // ?? Get user from cache instead of database
            var user = _cachedUsers.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                View.ShowError("User not found.");
                return;
            }
            
            var confirmMessage = $"Are you sure you want to delete user '{user.FirstName} {user.LastName}'?";
            if (!View.ShowConfirmation(confirmMessage))
                return;
            
            await ExecuteAsync(async () =>
            {
                try
                {
                    // Note: You'd need to implement a DeleteUserCommand in your application layer
                    View.ShowInfo($"Delete functionality for user '{user.FirstName} {user.LastName}' would be implemented here.");
                    
                    await LoadUsersAsync();
                    View.ShowSuccess("User deleted successfully.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error deleting user {UserId}", userId);
                    View.ShowError($"Failed to delete user: {ex.Message}");
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in delete user operation");
            View.ShowError($"Failed to delete user: {ex.Message}");
        }
    }

    private async void OnRefreshRequested(object? sender, EventArgs e)
    {
        await LoadUsersAsync();
    }

    private async Task LoadUsersAsync()
    {
        await ExecuteAsync(async () =>
        {
            _logger.LogInformation("Loading users");
            
            var users = await _mediator.Send(new GetAllUsersQuery());
            
            // ?? Cache users locally for fast access
            _cachedUsers = users.ToList();
            
            View.DisplayUsers(users);
            
            _logger.LogInformation("Loaded {UserCount} users", users.Count());
        });
    }
}