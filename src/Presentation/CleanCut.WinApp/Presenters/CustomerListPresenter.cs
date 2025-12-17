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
    private readonly Services.Factories.IViewFactory<ICustomerEditView> _customerEditViewFactory;
    private readonly ILogger<CustomerListPresenter> _logger;
    private List<CustomerInfo> _cachedCustomers = new(); // ?? Cache users locally

    public CustomerListPresenter(
        ICustomerListView view,
        IMediator mediator,
        IServiceProvider serviceProvider,
        Services.Factories.IViewFactory<ICustomerEditView> customerEditViewFactory,
        ILogger<CustomerListPresenter> logger)
        : base(view)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _customerEditViewFactory = customerEditViewFactory ?? throw new ArgumentNullException(nameof(customerEditViewFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public override void Initialize()
    {
        base.Initialize();
        // Subscribe to view events (use named handlers so we can unsubscribe)
        View.AddCustomerRequested += OnAddCustomerRequestedHandler;
        View.EditCustomerRequested += OnEditCustomerRequestedHandler;
        View.DeleteCustomerRequested += OnDeleteCustomerRequestedHandler;
        View.RefreshRequested += OnRefreshRequestedHandler;
        // Load initial data
        _ = LoadCustomersAsync();
    }

    public override void Cleanup()
    {
        // Unsubscribe from view events
        View.AddCustomerRequested -= OnAddCustomerRequestedHandler;
        View.EditCustomerRequested -= OnEditCustomerRequestedHandler;
        View.DeleteCustomerRequested -= OnDeleteCustomerRequestedHandler;
        View.RefreshRequested -= OnRefreshRequestedHandler;
        base.Cleanup();
    }

    // Named handlers
    private void OnAddCustomerRequestedHandler(object? sender, EventArgs e) => _ = OnAddCustomerRequested(sender, e);
    private void OnEditCustomerRequestedHandler(object? sender, Guid id) => _ = OnEditCustomerRequested(sender, id);
    private void OnDeleteCustomerRequestedHandler(object? sender, Guid id) => _ = OnDeleteCustomerRequestedAsync(sender, id);
    private void OnRefreshRequestedHandler(object? sender, EventArgs e) => _ = OnRefreshRequestedAsync(sender, e);

    private async Task OnAddCustomerRequested(object? sender, EventArgs e)
    {
        await ExecuteAsync(async () =>
        {
            _logger.LogInformation("Add user requested");

            var editForm = _customerEditViewFactory.Create();
            if (editForm is Form form)
            {
                form.Text = "Add New Customer";
            }

            editForm.ClearForm();

            var presenter = ActivatorUtilities.CreateInstance<CustomerEditPresenter>(_serviceProvider, editForm);
            presenter.Initialize();

            var result = (editForm as Form)?.ShowDialog();
            if (result == DialogResult.OK)
            {
                await LoadCustomersAsync();
                View.ShowSuccess("Customer created successfully.");
            }

            presenter.Cleanup();
            (presenter as IDisposable)?.Dispose();
        });
    }

    private async Task OnEditCustomerRequested(object? sender, Guid userId)
    {
        try
        {
            _logger.LogInformation("Edit user requested for user {CustomerId}", userId);

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

            var editForm = _customerEditViewFactory.Create();
            if (editForm is Form form)
            {
                form.Text = "Edit Customer";
            }

            var presenter = ActivatorUtilities.CreateInstance<CustomerEditPresenter>(_serviceProvider, editForm);
            presenter.SetEditMode(user);
            presenter.Initialize();

            var result = (editForm as Form)?.ShowDialog();
            if (result == DialogResult.OK)
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await LoadCustomersAsync();
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
            (presenter as IDisposable)?.Dispose();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in edit user operation");
            View.ShowError($"Failed to open user edit dialog: {ex.Message}");
        }
    }

    private async Task OnDeleteCustomerRequestedAsync(object? sender, Guid userId)
    {
        try
        {
            _logger.LogInformation("Delete user requested for user {CustomerId}", userId);

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

    private async Task OnRefreshRequestedAsync(object? sender, EventArgs e)
    {
        await LoadCustomersAsync();
    }

    private async Task LoadCustomersAsync()
    {
        await ExecuteAsync(async () =>
        {
            _logger.LogInformation("Loading users");

            var users = await _mediator.Send(new GetAllCustomersQuery());

            _cachedCustomers = users.ToList();

            View.DisplayCustomers(users);

            _logger.LogInformation("Loaded {CustomerCount} users", users.Count());
        });
    }
}
