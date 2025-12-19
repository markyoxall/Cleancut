using System;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using CleanCut.WinApp.Views.Customers;
using CleanCut.WinApp.Views.Products;
using CleanCut.WinApp.Presenters;
using CleanCut.WinApp.MVP;

namespace CleanCut.WinApp.Services.Navigation;

/// <summary>
/// Navigation service that manages MDI navigation and presenter lifecycle.
/// </summary>
internal class NavigationService : INavigationService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<NavigationService> _logger;

    private object? _currentPresenter;
    private IServiceScope? _currentScope;

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

            // Create a scope for the presenter and its scoped dependencies (DbContext, repos, unit of work)
            _currentScope = _serviceProvider.CreateScope();
            var scopedProvider = _currentScope.ServiceProvider;

            var userListView = scopedProvider.GetRequiredService<ICustomerListView>();
            var presenter = ActivatorUtilities.CreateInstance<CustomerListPresenter>(scopedProvider, userListView);
            _currentPresenter = presenter;
            presenter.Initialize();

            if (userListView is Form form)
            {
                form.MdiParent = mdiParent;
                // Attach a FormClosed handler to ensure the presenter and its scope
                // are cleaned up immediately when the user closes the child form.
                FormClosedEventHandler? closedHandler = null;
                closedHandler = (s, e) =>
                {
                    try
                    {
                        // Attempt presenter cleanup/dispose
                        if (_currentPresenter is BasePresenter<IView> bp)
                        {
                            bp.Cleanup();
                        }

                        if (_currentPresenter is IDisposable d)
                        {
                            d.Dispose();
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error during presenter cleanup from FormClosed handler");
                    }
                    finally
                    {
                        _currentPresenter = null;

                        try
                        {
                            _currentScope?.Dispose();
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Error disposing service scope from FormClosed handler");
                        }
                        finally
                        {
                            _currentScope = null;
                        }

                        // Unsubscribe handler
                        form.FormClosed -= closedHandler!;
                    }
                };

                form.FormClosed += closedHandler;
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

            _currentScope = _serviceProvider.CreateScope();
            var scopedProvider = _currentScope.ServiceProvider;

            var productListView = scopedProvider.GetRequiredService<IProductListView>();
            var presenter = ActivatorUtilities.CreateInstance<ProductListPresenter>(scopedProvider, productListView);
            _currentPresenter = presenter;
            presenter.Initialize();

            if (productListView is Form form)
            {
                form.MdiParent = mdiParent;
                // Attach a FormClosed handler to ensure the presenter and its scope
                // are cleaned up immediately when the user closes the child form.
                FormClosedEventHandler? closedHandler = null;
                closedHandler = (s, e) =>
                {
                    try
                    {
                        if (_currentPresenter is BasePresenter<IView> bp)
                        {
                            bp.Cleanup();
                        }

                        if (_currentPresenter is IDisposable d)
                        {
                            d.Dispose();
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error during presenter cleanup from FormClosed handler");
                    }
                    finally
                    {
                        _currentPresenter = null;

                        try
                        {
                            _currentScope?.Dispose();
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Error disposing service scope from FormClosed handler");
                        }
                        finally
                        {
                            _currentScope = null;
                        }

                        // Unsubscribe handler
                        form.FormClosed -= closedHandler!;
                    }
                };

                form.FormClosed += closedHandler;
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

        // Dispose the scope so scoped services (DbContext, repositories, unit of work) are disposed
        if (_currentScope != null)
        {
            try
            {
                _currentScope.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error disposing service scope");
            }
            finally
            {
                _currentScope = null;
            }
        }
    }
}
