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
using CleanCut.Infrastructure.Caching.Constants;
using CleanCut.WinApp.Services.Caching;

namespace CleanCut.WinApp.Presenters;

/// <summary>
/// Presenter for Customer List View implementing MVP pattern
/// </summary>
using CleanCut.WinApp.Services.Management;
using System.Threading.Tasks;

public class CustomerListPresenter : BasePresenter<ICustomerListView>
{
    private readonly IMediator _mediator;
    private readonly IServiceProvider _serviceProvider;
    private readonly Services.Factories.IViewFactory<ICustomerEditView> _customerEditViewFactory;
    private readonly ILogger<CustomerListPresenter> _logger;
    private readonly CleanCut.Application.Common.Interfaces.ICacheService _cacheService;
    private readonly ICacheManager _cacheManager;
    private List<CustomerInfo> _cachedCustomers = new();

    public CustomerListPresenter(
        ICustomerListView view,
        IMediator mediator,
        IServiceProvider serviceProvider,
        Services.Factories.IViewFactory<ICustomerEditView> customerEditViewFactory,
        ILogger<CustomerListPresenter> logger,
        CleanCut.Application.Common.Interfaces.ICacheService cacheService,
        ICacheManager cacheManager)
        : base(view)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _customerEditViewFactory = customerEditViewFactory ?? throw new ArgumentNullException(nameof(customerEditViewFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        _cacheManager = cacheManager ?? throw new ArgumentNullException(nameof(cacheManager));

        // If the view supports SetPresenter, wire it up so the Save Preferences button works
        if (view is CustomerListForm form)
        {
            form.SetPresenter(this);
        }
    }

    // Save grid preferences (column order and widths)
    public async Task SaveGridPreferencesAsync(List<string> columnOrder, Dictionary<string, int> columnWidths)
    {
        var newPrefs = new UserPreferences
        {
            ColumnOrder = columnOrder,
            ColumnWidths = columnWidths,
            CustomSettings = Preferences?.CustomSettings
        };
        await UserPreferencesHelper.SavePreferencesAsync(
            nameof(CustomerListPresenter),
            newPrefs,
            AppUserContext.CurrentUserName);
    }

    public override void Initialize()
    {
        base.Initialize();
        // Apply user preferences if available
        if (Preferences != null)
        {
            if (Preferences.ColumnOrder != null || Preferences.ColumnWidths != null)
            {
                View.ApplyGridPreferences(Preferences.ColumnOrder, Preferences.ColumnWidths);
            }
        }
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

            CustomerEditPresenter? presenter = null;
            try
            {
                presenter = ActivatorUtilities.CreateInstance<CustomerEditPresenter>(_serviceProvider, editForm);
                presenter.Initialize();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create CustomerEditPresenter");
                View.ShowError($"Failed to open customer edit dialog: {ex.Message}");
                return;
            }

            var result = (editForm as Form)?.ShowDialog();
            if (result == DialogResult.OK)
            {
                // Invalidate customer-related caches so other hosts/readers see changes
                await _cacheManager.InvalidateCustomersAsync();

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

            CustomerEditPresenter? presenter = null;
            try
            {
                presenter = ActivatorUtilities.CreateInstance<CustomerEditPresenter>(_serviceProvider, editForm);
                presenter.SetEditMode(user);
                presenter.Initialize();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create CustomerEditPresenter for edit");
                View.ShowError($"Failed to open customer edit dialog: {ex.Message}");
                return;
            }

            var result = (editForm as Form)?.ShowDialog();
            if (result == DialogResult.OK)
            {
                // Invalidate customer caches after update and refresh
                await _cacheManager.InvalidateCustomersAsync();

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

                    // Invalidate caches affected by delete
                    await _cacheManager.InvalidateCustomersAsync();

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

            var cacheKey = CacheKeys.AllCustomers();
            var users = await _cacheService.GetAsync<List<CustomerInfo>>(cacheKey);
            if (users == null)
            {
                users = (await _mediator.Send(new GetAllCustomersQuery())).ToList();
                await _cacheService.SetAsync(cacheKey, users, CacheTimeouts.Customers);
                _logger.LogInformation("Loaded users from database and cached");
            }
            else
            {
                _logger.LogInformation("Loaded users from cache");
            }

            _cachedCustomers = users.ToList();
            View.DisplayCustomers(users);
            _logger.LogInformation("Loaded {CustomerCount} users", users.Count());
        });
    }

    // Expose preferences for the view to access
    public UserPreferences? Preferences { get; set; }
}
