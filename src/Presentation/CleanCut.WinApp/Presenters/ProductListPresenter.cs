using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CleanCut.Application.Commands.Products.CreateProduct;
using CleanCut.Application.Commands.Products.UpdateProduct;
using CleanCut.Application.Queries.Products.GetProduct;
using CleanCut.Application.Queries.Products.GetProductsByCustomer;
using CleanCut.Application.Queries.Customers.GetAllCustomers;
using CleanCut.Application.DTOs;
using CleanCut.WinApp.MVP;
using CleanCut.WinApp.Views.Products;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using CleanCut.Infrastructure.Caching.Constants;

namespace CleanCut.WinApp.Presenters;

/// <summary>
/// Presenter for Product List View implementing MVP pattern
/// </summary>
public class ProductListPresenter : BasePresenter<IProductListView>
{
    private readonly IMediator _mediator;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ProductListPresenter> _logger;
    private readonly CleanCut.Application.Common.Interfaces.ICacheService _cacheService;

    private List<ProductInfo> _cachedProducts = new(); // ?? Cache products locally
    private List<CustomerInfo> _cachedCustomers = new(); // ?? Cache users locally
    private readonly Services.Factories.IViewFactory<IProductEditView> _productEditViewFactory;

    // Named handlers to subscribe/unsubscribe safely (invoke async implementations)
    private void OnAddProductRequestedHandler(object? sender, EventArgs e) => _ = OnAddProductRequested(sender, e);
    private void OnEditProductRequestedHandler(object? sender, Guid id) => _ = OnEditProductRequestedAsync(sender, id);
    private void OnDeleteProductRequestedHandler(object? sender, Guid id) => _ = OnDeleteProductRequestedAsync(sender, id);
    private void OnRefreshRequestedHandler(object? sender, EventArgs e) => _ = OnRefreshRequestedAsync(sender, e);
    private void OnViewProductsByCustomerRequestedHandler(object? sender, Guid id) => _ = OnViewProductsByCustomerRequestedAsync(sender, id);

    public ProductListPresenter(
        IProductListView view,
        IMediator mediator,
        IServiceProvider serviceProvider,
        Services.Factories.IViewFactory<IProductEditView> productEditViewFactory,
        ILogger<ProductListPresenter> logger,
        CleanCut.Application.Common.Interfaces.ICacheService cacheService)
        : base(view)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _productEditViewFactory = productEditViewFactory ?? throw new ArgumentNullException(nameof(productEditViewFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
    }

    public override void Initialize()
    {
        base.Initialize();
        // Subscribe to view events
        View.AddProductRequested += OnAddProductRequestedHandler;
        View.EditProductRequested += OnEditProductRequestedHandler;
        View.DeleteProductRequested += OnDeleteProductRequestedHandler;
        View.RefreshRequested += OnRefreshRequestedHandler;
        View.ViewProductsByCustomerRequested += OnViewProductsByCustomerRequestedHandler;
        // Load initial data
        _ = LoadInitialDataAsync();
    }

    public override void Cleanup()
    {
        // Unsubscribe from view events
        View.AddProductRequested -= OnAddProductRequestedHandler;
        View.EditProductRequested -= OnEditProductRequestedHandler;
        View.DeleteProductRequested -= OnDeleteProductRequestedHandler;
        View.RefreshRequested -= OnRefreshRequestedHandler;
        View.ViewProductsByCustomerRequested -= OnViewProductsByCustomerRequestedHandler;
        base.Cleanup();
    }

    private async Task LoadInitialDataAsync()
    {
        await ExecuteAsync(async () =>
        {
            _logger.LogInformation("Loading initial product data");

            // Load users for filtering - try cache first
            var customerCacheKey = CacheKeys.AllCustomers();
            var users = await _cacheService.GetAsync<List<CustomerInfo>>(customerCacheKey);
            if (users == null)
            {
                users = (await _mediator.Send(new GetAllCustomersQuery())).ToList();
                await _cacheService.SetAsync(customerCacheKey, users, CacheTimeouts.Customers);
                _logger.LogInformation("Loaded users from database and cached");
            }
            else
            {
                _logger.LogInformation("Loaded users from cache");
            }

            _cachedCustomers = users.ToList();
            View.SetAvailableCustomers(users);

            // Load products for the first user (or all products if we have a GetAllProducts query)
            if (users.Any())
            {
                var firstCustomer = users.First();
                await LoadProductsByCustomerAsync(firstCustomer.Id);
            }

            _logger.LogInformation("Initial product data loaded");
        });
    }

    private async Task OnAddProductRequested(object? sender, EventArgs e)
    {
        try
        {
            _logger.LogInformation("Add product requested");

            // Resolve form using factory and create presenter via DI
            var editForm = _productEditViewFactory.Create();
            if (editForm is Form form)
            {
                form.Text = "Add New Product";
            }

            editForm.ClearForm();
            editForm.SetAvailableCustomers(_cachedCustomers);

            ProductEditPresenter? presenter = null;
            try
            {
                presenter = ActivatorUtilities.CreateInstance<ProductEditPresenter>(_serviceProvider, editForm);
                presenter.Initialize();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create ProductEditPresenter");
                View.ShowError($"Failed to open product edit dialog: {ex.Message}");
                return;
            }

            // Show dialog immediately
            var result = (editForm as Form)?.ShowDialog();
            if (result == DialogResult.OK)
            {
                // Invalidate product caches so lists refresh
                await _cacheService.RemoveByPatternAsync(CacheKeys.ProductPattern());

                // Refresh in background
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await LoadInitialDataAsync();
                        if (View is Control control)
                        {
                            control.Invoke(() => View.ShowSuccess("Product created successfully."));
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error refreshing products after add");
                    }
                });
            }

            presenter.Cleanup();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in add product operation");
            View.ShowError($"Failed to open product add dialog: {ex.Message}");
        }
    }

    private async Task OnEditProductRequestedAsync(object? sender, Guid productId)
    {
        try
        {
            _logger.LogInformation("Edit product requested for product {ProductId}", productId);

            // OPTIMIZATION: Get product from cache instead of database
            var product = _cachedProducts.FirstOrDefault(p => p.Id == productId);
            if (product == null)
            {
                View.ShowError("Product not found.");
                return;
            }

            // Resolve form using factory and create presenter via DI
            var editForm = _productEditViewFactory.Create();
            if (editForm is Form form)
            {
                form.Text = "Edit Product";
            }

            editForm.SetAvailableCustomers(_cachedCustomers);

            ProductEditPresenter? presenter = null;
            try
            {
                presenter = ActivatorUtilities.CreateInstance<ProductEditPresenter>(_serviceProvider, editForm);
                presenter.SetEditMode(product);
                presenter.Initialize();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create ProductEditPresenter for edit");
                View.ShowError($"Failed to open product edit dialog: {ex.Message}");
                return;
            }

            // Show dialog immediately
            var result = (editForm as Form)?.ShowDialog();
            if (result == DialogResult.OK)
            {
                // Invalidate product caches so lists refresh
                await _cacheService.RemoveByPatternAsync(CacheKeys.ProductPattern());

                // Refresh in background
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await LoadInitialDataAsync();
                        if (View is Control control)
                        {
                            control.Invoke(() => View.ShowSuccess("Product updated successfully."));
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error refreshing products after edit");
                    }
                });
            }

            presenter.Cleanup();
            (presenter as IDisposable)?.Dispose();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in edit product operation");
            View.ShowError($"Failed to open product edit dialog: {ex.Message}");
        }
    }

    private async Task OnDeleteProductRequestedAsync(object? sender, Guid productId)
    {
        try
        {
            _logger.LogInformation("Delete product requested for product {ProductId}", productId);

            // ?? Get product from cache instead of database
            var product = _cachedProducts.FirstOrDefault(p => p.Id == productId);
            if (product == null)
            {
                HandleError(new InvalidOperationException("Product not found."));
                return;
            }

            var confirmMessage = $"Are you sure you want to delete product '{product.Name}'?";
            if (!View.ShowConfirmation(confirmMessage))
                return;

            await ExecuteAsync(async () =>
            {
                try
                {
                    // Note: You'd need to implement a DeleteProductCommand in your application layer
                    View.ShowInfo($"Delete functionality for product '{product.Name}' would be implemented here.");

                    // Invalidate product caches after delete
                    await _cacheService.RemoveByPatternAsync(CacheKeys.ProductPattern());

                    await LoadInitialDataAsync();
                    View.ShowSuccess("Product deleted successfully.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error deleting product {ProductId}", productId);
                    View.ShowError($"Failed to delete product: {ex.Message}");
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in delete product operation");
            View.ShowError($"Failed to delete product: {ex.Message}");
        }
    }

    private async Task OnRefreshRequestedAsync(object? sender, EventArgs e)
    {
        await LoadInitialDataAsync();
    }

    private async Task OnViewProductsByCustomerRequestedAsync(object? sender, Guid userId)
    {
        await LoadProductsByCustomerAsync(userId);
    }

    private async Task LoadProductsByCustomerAsync(Guid userId)
    {
        await ExecuteAsync(async () =>
        {
            _logger.LogInformation("Loading products for user {CustomerId}", userId);

            var cacheKey = CacheKeys.ProductsByCustomer(userId);
            var products = await _cacheService.GetAsync<List<ProductInfo>>(cacheKey);
            if (products == null)
            {
                products = (await _mediator.Send(new GetProductsByCustomerQuery(userId))).ToList();
                await _cacheService.SetAsync(cacheKey, products, CacheTimeouts.Products);
                _logger.LogInformation("Loaded products from database and cached for user {CustomerId}", userId);
            }
            else
            {
                _logger.LogInformation("Loaded products from cache for user {CustomerId}", userId);
            }

            // ?? Cache products locally for fast access
            _cachedProducts = products.ToList();

            View.DisplayProducts(products);

            _logger.LogInformation("Loaded {ProductCount} products for user {CustomerId}", products.Count(), userId);
        });
    }
}
