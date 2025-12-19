using CleanCut.WinApp.MVP;
using CleanCut.WinApp.Presenters;
using CleanCut.WinApp.Views.Countries;
using CleanCut.WinApp.Views.Customers;
using CleanCut.WinApp.Views.Products;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CleanCut.WinApp;

/// <summary>
/// Main application form with navigation
/// </summary>
public partial class MainForm : BaseForm
{
    private readonly Services.Management.IManagementLoader _managementLoader;
    private readonly ILogger<MainForm> _logger;
    private Services.Management.ILoadedManagement? _activeManagement;

    public MainForm(Services.Management.IManagementLoader managementLoader, ILogger<MainForm> logger)
    {
        _managementLoader = managementLoader ?? throw new ArgumentNullException(nameof(managementLoader));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        InitializeComponent();
        // Cannot call async method direct      ly in constructor, so use Load event
        this.Load += MainForm_Load;
    }

    private async void MainForm_Load(object? sender, EventArgs e)
    {
        await InitializeNavigationAsync();
    }


    private async Task InitializeNavigationAsync()
    {
        // Load the customer management by default
        await LoadManagementAsync<ICustomerListView, CustomerListPresenter>("customer");
    }

    private async Task LoadManagementAsync<TView, TPresenter>(string managementName)
        where TView : class, CleanCut.WinApp.MVP.IView
        where TPresenter : CleanCut.WinApp.MVP.BasePresenter<TView>
    {
        try
        {
            // Dispose previous management (presenter + scope) before loading new
            if (_activeManagement?.Presenter is IDisposable disposablePresenter)
                disposablePresenter.Dispose();
            _activeManagement?.Scope.Dispose();

            _activeManagement = await _managementLoader.LoadAsync<TView, TPresenter>();

            if (_activeManagement.View is Form form)
            {
                form.MdiParent = this;
                form.Show();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error loading {managementName} management");
            ShowError($"Failed to load {managementName} management: {ex.Message}");
        }
    }
    
    private void CleanupPresenters()
    {
        // Close and dispose any existing MDI child windows
        foreach (Form childForm in MdiChildren)
        {
            try
            {
                childForm.Close();
                childForm.Dispose(); // Explicitly dispose
            }
            catch (Exception ex)
            {
                // Log non-fatal errors when closing MDI children to aid debugging
                _logger.LogDebug(ex, "Non-fatal error closing MDI child during cleanup");
                MessageBox.Show(this, $"Error closing MDI child form: {ex.Message}", "Cleanup Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // Clean up active management handle (presenter + scope)
        if (_activeManagement != null)
        {
            try
            {
                _activeManagement.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error disposing active management handle");
                MessageBox.Show(this, $"Error disposing active management: {ex.Message}", "Disposal Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                _activeManagement = null;
            }
        }
    }

    private async void OnCustomerManagementClicked(object? sender, EventArgs e)
    {
        await LoadManagementAsync<ICustomerListView, CustomerListPresenter>("customer");
    }

    private async void OnProductManagementClicked(object? sender, EventArgs e)
    {
        await LoadManagementAsync<IProductListView, ProductListPresenter>("product");
    }

    private async void OnCountryManagementClicked(object? sender, EventArgs e)
    {
        await LoadManagementAsync<ICountryListView, CountryListPresenter>("country");
    }

    private void OnExitClicked(object? sender, EventArgs e)
    {
        Close();
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        CleanupPresenters();
        base.OnFormClosed(e);
    }




}
