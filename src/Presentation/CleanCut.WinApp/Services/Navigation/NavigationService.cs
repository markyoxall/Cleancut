using System;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using CleanCut.WinApp.Views.Customers;
using CleanCut.WinApp.Views.Products;
using CleanCut.WinApp.Presenters;
using CleanCut.WinApp.MVP;

namespace CleanCut.WinApp.Services.Navigation;

internal class NavigationService : INavigationService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<NavigationService> _logger;

    private object? _currentPresenter;

    public NavigationService(IServiceProvider serviceProvider, ILogger<NavigationService> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void ShowCustomerManagement(Form mdiParent)
    {
        try
        {
            _logger.LogInformation("Navigation: ShowCustomerManagement");

            CloseAllChildren(mdiParent);

            var userListView = _serviceProvider.GetRequiredService<ICustomerListView>();
            var presenter = ActivatorUtilities.CreateInstance<CustomerListPresenter>(_serviceProvider, userListView);
            _currentPresenter = presenter;
            presenter.Initialize();

            if (userListView is Form form)
            {
                form.MdiParent = mdiParent;
                form.Show();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error showing customer management");
            if (mdiParent is BaseForm bf)
                bf.ShowError($"Failed to load customer management: {ex.Message}");
        }
    }

    public void ShowProductManagement(Form mdiParent)
    {
        try
        {
            _logger.LogInformation("Navigation: ShowProductManagement");

            CloseAllChildren(mdiParent);

            var productListView = _serviceProvider.GetRequiredService<IProductListView>();
            var presenter = ActivatorUtilities.CreateInstance<ProductListPresenter>(_serviceProvider, productListView);
            _currentPresenter = presenter;
            presenter.Initialize();

            if (productListView is Form form)
            {
                form.MdiParent = mdiParent;
                form.Show();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error showing product management");
            if (mdiParent is BaseForm bf)
                bf.ShowError($"Failed to load product management: {ex.Message}");
        }
    }

    public void CloseAllChildren(Form mdiParent)
    {
        // Close MDI children
        foreach (Form child in mdiParent.MdiChildren)
        {
            try
            {
                child.Close();
                child.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error closing child form");
            }
        }

        // Cleanup presenter
        if (_currentPresenter is BasePresenter<IView> bp)
        {
            try
            {
                bp.Cleanup();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error during presenter cleanup");
            }
        }

        if (_currentPresenter is IDisposable d)
        {
            try
            {
                d.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error disposing presenter");
            }
        }

        _currentPresenter = null;
    }
}
