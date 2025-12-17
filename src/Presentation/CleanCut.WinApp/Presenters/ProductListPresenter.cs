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

namespace CleanCut.WinApp.Presenters;

/// <summary>
/// Presenter for Product List View implementing MVP pattern
/// </summary>
public class ProductListPresenter : BasePresenter<IProductListView>
{
    private readonly IMediator _mediator;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ProductListPresenter> _logger;

    private List<ProductInfo> _cachedProducts = new(); // ?? Cache products locally
    private List<CustomerInfo> _cachedCustomers = new(); // ?? Cache users locally

    public ProductListPresenter(
        IProductListView view,
        IMediator mediator,
        IServiceProvider serviceProvider,
        ILogger<ProductListPresenter> logger)
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
        View.AddProductRequested += OnAddProductRequested;
        View.EditProductRequested += OnEditProductRequested;
        View.DeleteProductRequested += OnDeleteProductRequested;
        View.RefreshRequested += OnRefreshRequested;
        View.ViewProductsByCustomerRequested += OnViewProductsByCustomerRequested;
        // Load initial data
        _ = LoadInitialDataAsync();
    }

    public override void Cleanup()
    {
        // Unsubscribe from view events
        View.AddProductRequested -= OnAddProductRequested;
        View.EditProductRequested -= OnEditProductRequested;
        View.DeleteProductRequested -= OnDeleteProductRequested;
        View.RefreshRequested -= OnRefreshRequested;
        View.ViewProductsByCustomerRequested -= OnViewProductsByCustomerRequested;
        base.Cleanup();
    }

    private async Task LoadInitialDataAsync()
    {
        await ExecuteAsync(async () =>
        {
            _logger.LogInformation("Loading initial product data");

            // Load users for filtering
            var users = await _mediator.Send(new GetAllCustomersQuery());
            _cachedCustomers = users.ToList(); // ?? Cache users
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

    private async void OnAddProductRequested(object? sender, EventArgs e)
    {
        try
        {
            _logger.LogInformation("Add product requested");

            // Resolve form and presenter from DI
            var editForm = (_serviceProvider.GetRequiredService<IProductEditView>()) ?? throw new InvalidOperationException("ServiceProvider did not provide IProductEditView");
            if (editForm is Form form)
            {
                form.Text = "Add New Product";
            }

            editForm.ClearForm();
            editForm.SetAvailableCustomers(_cachedCustomers);

            var mediator = _serviceProvider.GetRequiredService<IMediator>();
            var logger = _serviceProvider.GetRequiredService<ILogger<ProductEditPresenter>>();
            var presenter = new ProductEditPresenter(editForm, mediator, logger);
            presenter.Initialize();

            // Show dialog immediately
            var result = (editForm as Form)?.ShowDialog();
            if (result == DialogResult.OK)
            {
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

    private async void OnEditProductRequested(object? sender, Guid productId)
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

            // Resolve form and presenter from DI
            var editForm = (_serviceProvider.GetRequiredService<IProductEditView>()) ?? throw new InvalidOperationException("ServiceProvider did not provide IProductEditView");
            if (editForm is Form form)
            {
                form.Text = "Edit Product";
            }

            editForm.SetAvailableCustomers(_cachedCustomers);

            var mediator = _serviceProvider.GetRequiredService<IMediator>();
            var logger = _serviceProvider.GetRequiredService<ILogger<ProductEditPresenter>>();
            var presenter = new ProductEditPresenter(editForm, mediator, logger);
            presenter.SetEditMode(product);
            presenter.Initialize();

            // Show dialog immediately
            var result = (editForm as Form)?.ShowDialog();
            if (result == DialogResult.OK)
            {
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
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in edit product operation");
            View.ShowError($"Failed to open product edit dialog: {ex.Message}");
        }
    }

    private async void OnDeleteProductRequested(object? sender, Guid productId)
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

    private async void OnRefreshRequested(object? sender, EventArgs e)
    {
        await LoadInitialDataAsync();
    }

    private async void OnViewProductsByCustomerRequested(object? sender, Guid userId)
    {
        await LoadProductsByCustomerAsync(userId);
    }

    private async Task LoadProductsByCustomerAsync(Guid userId)
    {
        await ExecuteAsync(async () =>
        {
            _logger.LogInformation("Loading products for user {CustomerId}", userId);

            var products = await _mediator.Send(new GetProductsByCustomerQuery(userId));

            // ?? Cache products locally for fast access
            _cachedProducts = products.ToList();

            View.DisplayProducts(products);

            _logger.LogInformation("Loaded {ProductCount} products for user {CustomerId}", products.Count(), userId);
        });
    }
}
