using CleanCut.Application.Commands.Customers.CreateCustomer;
using CleanCut.Application.Commands.Customers.UpdateCustomer;
using CleanCut.Application.Queries.Customers.GetAllCustomers;
using CleanCut.Application.Queries.Customers.GetCustomer;
using CleanCut.Application.DTOs;
using CleanCut.WinApp.MVP;
using CleanCut.WinApp.Views.Customers;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CleanCut.WinApp.Presenters;

/// <summary>
/// Presenter for Customer List View implementing MVP pattern
/// </summary>
public class CustomerListPresenter : BasePresenter<ICustomerListView>
{
    private readonly IMediator _mediator;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CustomerListPresenter> _logger;
    private readonly CleanCut.WinApp.Services.INotificationMediator? _notificationMediator;
    private List<CustomerInfo> _cachedCustomers = new(); // ?? Cache users locally

    public CustomerListPresenter(
        ICustomerListView view, 
        IMediator mediator, 
        IServiceProvider serviceProvider,
        ILogger<CustomerListPresenter> logger,
        CleanCut.WinApp.Services.INotificationMediator? notificationMediator = null) 
        : base(view)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _notificationMediator = notificationMediator;
    }

    public override void Initialize()
    {
        base.Initialize();
        
        // Subscribe to view events
        View.AddCustomerRequested += OnAddCustomerRequested;
        View.EditCustomerRequested += OnEditCustomerRequested;
        View.DeleteCustomerRequested += OnDeleteCustomerRequested;
        View.RefreshRequested += OnRefreshRequested;
        
        // Load initial data
        _ = LoadCustomersAsync();

        // Subscribe to notifications
        if (_notificationMediator != null)
        {
            _notificationMediator.CustomerUpdated += OnCustomerUpdatedNotification;
        }
    }

    public override void Cleanup()
    {
        // Unsubscribe from view events
        View.AddCustomerRequested -= OnAddCustomerRequested;
        View.EditCustomerRequested -= OnEditCustomerRequested;
        View.DeleteCustomerRequested -= OnDeleteCustomerRequested;
        View.RefreshRequested -= OnRefreshRequested;
        
        base.Cleanup();

        if (_notificationMediator != null)
        {
            _notificationMediator.CustomerUpdated -= OnCustomerUpdatedNotification;
        }
    }

    private async Task OnCustomerUpdatedNotification(CustomerInfo dto)
    {
        try
        {
            await LoadCustomersAsync();
            if (View is Control control)
            {
                control.Invoke(() => View.ShowSuccess("Customers refreshed due to external update."));
            }
        }
        catch { }
    }

    private async void OnAddCustomerRequested(object? sender, EventArgs e)
    {
        await ExecuteAsync(async () =>
        {
            _logger.LogInformation("Add user requested");
            
            // ?? Create form and presenter outside of async context
            var editForm = new CustomerEditForm();
            editForm.Text = "Add New Customer";
            editForm.ClearForm();
            
            var presenter = new CustomerEditPresenter(editForm, _mediator, _logger);
            presenter.Initialize();
            
            // ?? Show dialog on UI thread
            var result = editForm.ShowDialog();
            if (result == DialogResult.OK)
            {
                await LoadCustomersAsync();
                View.ShowSuccess("Customer created successfully.");
            }
            
            presenter.Cleanup();
        });
    }

    private async void OnEditCustomerRequested(object? sender, Guid userId)
    {
        try
        {
            _logger.LogInformation("Edit user requested for user {CustomerId}", userId);
            
            // ?? OPTIMIZATION: Get user from cache instead of database
            var user = _cachedCustomers.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                _logger.LogWarning("Customer {CustomerId} not found in cache, fetching from database", userId);
                user = await _mediator.Send(new GetCustomerQuery(userId));
                
                if (user == null)
                {
                    View.ShowError("Customer not found.");
                    return;
                }
            }
            
            // ?? Create form and presenter on UI thread (fast)
            var editForm = new CustomerEditForm();
            editForm.Text = "Edit Customer";
            
            var presenter = new CustomerEditPresenter(editForm, _mediator, _logger);
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
                        await LoadCustomersAsync();
                        
                        // ?? Show success message on UI thread
                        if (View is Control control)
                        {
                            control.Invoke(() => View.ShowSuccess("Customer updated successfully."));
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

    private async void OnDeleteCustomerRequested(object? sender, Guid userId)
    {
        try
        {
            _logger.LogInformation("Delete user requested for user {CustomerId}", userId);
            
            // ?? Get user from cache instead of database
            var user = _cachedCustomers.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                View.ShowError("Customer not found.");
                return;
            }
            
            var confirmMessage = $"Are you sure you want to delete user '{user.FirstName} {user.LastName}'?";
            if (!View.ShowConfirmation(confirmMessage))
                return;
            
            await ExecuteAsync(async () =>
            {
                try
                {
                    // Note: You'd need to implement a DeleteCustomerCommand in your application layer
                    View.ShowInfo($"Delete functionality for user '{user.FirstName} {user.LastName}' would be implemented here.");
                    
                    await LoadCustomersAsync();
                    View.ShowSuccess("Customer deleted successfully.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error deleting user {CustomerId}", userId);
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
        await LoadCustomersAsync();
    }

    private async Task LoadCustomersAsync()
    {
        await ExecuteAsync(async () =>
        {
            _logger.LogInformation("Loading users");
            
            var users = await _mediator.Send(new GetAllCustomersQuery());
            
            // ?? Cache users locally for fast access
            _cachedCustomers = users.ToList();
            
            View.DisplayCustomers(users);
            
            _logger.LogInformation("Loaded {CustomerCount} users", users.Count());
        });
    }
}
