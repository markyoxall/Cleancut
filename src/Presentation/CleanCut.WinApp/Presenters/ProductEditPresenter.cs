using CleanCut.Application.Commands.Products.CreateProduct;
using CleanCut.Application.Commands.Products.UpdateProduct;
using CleanCut.Application.DTOs;
using CleanCut.WinApp.MVP;
using CleanCut.WinApp.Views.Products;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CleanCut.WinApp.Presenters;

/// <summary>
/// Presenter for Product Edit View implementing MVP pattern
/// </summary>
public class ProductEditPresenter : BasePresenter<IProductEditView>
{
    private readonly IMediator _mediator;
    private readonly ILogger _logger;
    private ProductDto? _existingProduct;
    private bool _isEditMode;

    public ProductEditPresenter(IProductEditView view, IMediator mediator, ILogger logger) 
        : base(view)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void SetEditMode(ProductDto product)
    {
        _existingProduct = product;
        _isEditMode = true;
        
        var editModel = new ProductEditModel
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            IsAvailable = product.IsAvailable,
            UserId = product.UserId
        };
        
        View.SetProductData(editModel);
    }

    public override void Initialize()
    {
        base.Initialize();
        
        // Subscribe to view events
        View.SaveRequested += OnSaveRequested;
        View.CancelRequested += OnCancelRequested;
        
        if (!_isEditMode)
        {
            View.ClearForm();
        }
    }

    public override void Cleanup()
    {
        // Unsubscribe from view events
        View.SaveRequested -= OnSaveRequested;
        View.CancelRequested -= OnCancelRequested;
        
        base.Cleanup();
    }

    private async void OnSaveRequested(object? sender, EventArgs e)
    {
        await ExecuteAsync(async () =>
        {
            // Validate form
            var validationErrors = View.ValidateForm();
            if (validationErrors.Any())
            {
                var errorMessage = string.Join("\n", validationErrors.Values);
                View.ShowError($"Please fix the following errors:\n{errorMessage}");
                return;
            }

            var productData = View.GetProductData();

            try
            {
                if (_isEditMode && _existingProduct != null)
                {
                    // Update existing product
                    _logger.LogInformation("Updating product {ProductId}", _existingProduct.Id);
                    
                    var updateCommand = new UpdateProductCommand(
                        _existingProduct.Id,
                        productData.Name,
                        productData.Description,
                        productData.Price
                    );
                    
                    await _mediator.Send(updateCommand);
                    _logger.LogInformation("Product updated successfully");
                }
                else
                {
                    // Create new product
                    _logger.LogInformation("Creating new product");
                    
                    var createCommand = new CreateProductCommand(
                        productData.Name,
                        productData.Description,
                        productData.Price,
                        productData.UserId
                    );
                    
                    await _mediator.Send(createCommand);
                    _logger.LogInformation("Product created successfully");
                }

                // Close the dialog with OK result
                if (View is Form form)
                {
                    form.DialogResult = DialogResult.OK;
                    form.Close();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving product");
                View.ShowError($"Failed to save product: {ex.Message}");
            }
        });
    }

    private void OnCancelRequested(object? sender, EventArgs e)
    {
        _logger.LogInformation("Product edit cancelled");
        
        if (View is Form form)
        {
            form.DialogResult = DialogResult.Cancel;
            form.Close();
        }
    }
}