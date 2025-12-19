using CleanCut.Application.Commands.Products.CreateProduct;
using CleanCut.Application.Commands.Products.UpdateProduct;
using CleanCut.Application.DTOs;
using CleanCut.WinApp.MVP;
using CleanCut.WinApp.Views.Products;
using MediatR;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace CleanCut.WinApp.Presenters;

/// <summary>
/// Presenter for Product Edit View implementing MVP pattern
/// </summary>
public class ProductEditPresenter : BasePresenter<IProductEditView>
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;
    private readonly ILogger<ProductEditPresenter> _logger;
    private readonly Services.ICommandFactory _commandFactory;
    private ProductInfo? _existingProduct;
    private bool _isEditMode;

    public ProductEditPresenter(IProductEditView view, IMediator mediator, IMapper mapper, Services.ICommandFactory commandFactory, ILogger<ProductEditPresenter> logger) 
        : base(view)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _commandFactory = commandFactory ?? throw new ArgumentNullException(nameof(commandFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void SetEditMode(ProductInfo product)
    {
        _existingProduct = product;
        _isEditMode = true;
        
        var editModel = _mapper.Map<ProductEditViewModel>(product);
        View.SetProductData(editModel);
    }

    public override void Initialize()
    {
        base.Initialize();
        
        // Subscribe to view events (use wrapper to avoid async void)
        View.SaveRequested += OnSaveRequestedHandler;
        View.CancelRequested += OnCancelRequested;
        
        if (!_isEditMode)
        {
            View.ClearForm();
        }
    }

    public override void Cleanup()
    {
        // Unsubscribe from view events
        View.SaveRequested -= OnSaveRequestedHandler;
        View.CancelRequested -= OnCancelRequested;
        
        base.Cleanup();
    }
    private void OnSaveRequestedHandler(object? sender, EventArgs e) => _ = OnSaveRequested(sender, e);

    private async Task OnSaveRequested(object? sender, EventArgs e)
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
            var dto = _mapper.Map<ProductInfo>(productData);

            try
            {
                if (_isEditMode && _existingProduct != null)
                {
                    // Update existing product
                    _logger.LogInformation("Updating product {ProductId}", _existingProduct.Id);
                    var updateCommand = _commandFactory.UpdateProductCommand(_existingProduct.Id, productData);
                    
                    await _mediator.Send(updateCommand);
                    _logger.LogInformation("Product updated successfully");
                }
                else
                {
                    // Create new product
                    _logger.LogInformation("Creating new product");
                    var createCommand = _commandFactory.CreateProductCommand(productData);
                    
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
