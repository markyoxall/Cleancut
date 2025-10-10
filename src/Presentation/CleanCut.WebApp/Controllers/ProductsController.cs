using AutoMapper;
using CleanCut.WebApp.Models.Products;
using CleanCut.WebApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace CleanCut.WebApp.Controllers;

/// <summary>
/// MVC Controller for Product management via API calls
/// </summary>
public class ProductsController : Controller
{
    private readonly IProductApiService _productApiService;
    private readonly IUserApiService _userApiService;
    private readonly IMapper _mapper;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(
        IProductApiService productApiService, 
        IUserApiService userApiService,
        IMapper mapper, 
        ILogger<ProductsController> logger)
    {
        _productApiService = productApiService ?? throw new ArgumentNullException(nameof(productApiService));
        _userApiService = userApiService ?? throw new ArgumentNullException(nameof(userApiService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Display list of products
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index(Guid? userId, string? searchTerm, bool? isAvailableFilter)
    {
        try
        {
            _logger.LogInformation("Loading products list with user filter: {UserId}, search: {SearchTerm}, available: {IsAvailableFilter}", 
                userId, searchTerm, isAvailableFilter);

            // Load users for filter dropdown
            var users = await _userApiService.GetAllUsersAsync();
            
            // Load products
            List<Application.DTOs.ProductDto> products;
            if (userId.HasValue)
            {
                products = (await _productApiService.GetProductsByUserAsync(userId.Value)).ToList();
            }
            else if (users.Any())
            {
                // If no specific user selected, get products for the first user
                var firstUser = users.First();
                products = (await _productApiService.GetProductsByUserAsync(firstUser.Id)).ToList();
            }
            else
            {
                products = new List<Application.DTOs.ProductDto>();
            }
            
            // Apply filters
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                products = products.Where(p => 
                    p.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    p.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            if (isAvailableFilter.HasValue)
            {
                products = products.Where(p => p.IsAvailable == isAvailableFilter.Value).ToList();
            }

            var viewModel = new ProductListViewModel
            {
                Products = products,
                Users = users.ToList(),
                SelectedUserId = userId,
                SearchTerm = searchTerm,
                IsAvailableFilter = isAvailableFilter,
                TotalProducts = products.Count
            };

            // Check for success message in TempData
            if (TempData.ContainsKey("SuccessMessage"))
            {
                viewModel.Message = TempData["SuccessMessage"]?.ToString() ?? string.Empty;
                viewModel.IsSuccess = true;
            }

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading products list");
            
            var errorViewModel = new ProductListViewModel
            {
                Message = "An error occurred while loading products. Please try again.",
                IsSuccess = false
            };
            
            return View(errorViewModel);
        }
    }

    /// <summary>
    /// Display product details
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Details(Guid id)
    {
        try
        {
            _logger.LogInformation("Loading details for product {ProductId}", id);

            var product = await _productApiService.GetProductByIdAsync(id);
            if (product == null)
            {
                _logger.LogWarning("Product {ProductId} not found", id);
                return NotFound("Product not found");
            }

            var owner = product.User ?? await _userApiService.GetUserByIdAsync(product.UserId);

            var viewModel = new ProductDetailsViewModel
            {
                Product = product,
                Owner = owner
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading product details for {ProductId}", id);
            return BadRequest("An error occurred while loading product details");
        }
    }

    /// <summary>
    /// Display create product form
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        try
        {
            var users = await _userApiService.GetAllUsersAsync();
            
            var viewModel = new ProductEditViewModel
            {
                AvailableUsers = users.ToList()
            };
            
            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading create product form");
            return BadRequest("An error occurred while loading the form");
        }
    }

    /// <summary>
    /// Handle create product form submission
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductEditViewModel model)
    {
        if (!ModelState.IsValid)
        {
            // Reload users for dropdown
            var users = await _userApiService.GetAllUsersAsync();
            model.AvailableUsers = users.ToList();
            return View(model);
        }

        try
        {
            _logger.LogInformation("Creating new product {ProductName} for user {UserId}", model.Name, model.UserId);

            var createdProduct = await _productApiService.CreateProductAsync(model.Name, model.Description, model.Price, model.UserId);

            _logger.LogInformation("Product created successfully with ID {ProductId}", createdProduct.Id);
            
            TempData["SuccessMessage"] = $"Product '{createdProduct.Name}' was created successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product {ProductName}", model.Name);
            
            ModelState.AddModelError(string.Empty, "An error occurred while creating the product. Please try again.");
            
            // Reload users for dropdown
            var users = await _userApiService.GetAllUsersAsync();
            model.AvailableUsers = users.ToList();
            return View(model);
        }
    }

    /// <summary>
    /// Display edit product form
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        try
        {
            _logger.LogInformation("Loading product {ProductId} for editing", id);

            var product = await _productApiService.GetProductByIdAsync(id);
            if (product == null)
            {
                _logger.LogWarning("Product {ProductId} not found for editing", id);
                return NotFound("Product not found");
            }

            var users = await _userApiService.GetAllUsersAsync();
            
            var viewModel = _mapper.Map<ProductEditViewModel>(product);
            viewModel.AvailableUsers = users.ToList();
            
            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading product {ProductId} for editing", id);
            return BadRequest("An error occurred while loading the product for editing");
        }
    }

    /// <summary>
    /// Handle edit product form submission
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, ProductEditViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest("ID mismatch");
        }

        if (!ModelState.IsValid)
        {
            // Reload users for dropdown
            var users = await _userApiService.GetAllUsersAsync();
            model.AvailableUsers = users.ToList();
            return View(model);
        }

        try
        {
            _logger.LogInformation("Updating product {ProductId}", id);

            var updatedProduct = await _productApiService.UpdateProductAsync(id, model.Name, model.Description, model.Price);

            _logger.LogInformation("Product {ProductId} updated successfully", id);
            
            TempData["SuccessMessage"] = $"Product '{updatedProduct.Name}' was updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product {ProductId}", id);
            
            ModelState.AddModelError(string.Empty, "An error occurred while updating the product. Please try again.");
            
            // Reload users for dropdown
            var users = await _userApiService.GetAllUsersAsync();
            model.AvailableUsers = users.ToList();
            return View(model);
        }
    }

    /// <summary>
    /// Display delete confirmation
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var product = await _productApiService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound("Product not found");
            }

            return View(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading product {ProductId} for deletion", id);
            return BadRequest("An error occurred while loading the product");
        }
    }

    /// <summary>
    /// Handle delete confirmation
    /// </summary>
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        try
        {
            var success = await _productApiService.DeleteProductAsync(id);
            if (!success)
            {
                return NotFound("Product not found");
            }

            _logger.LogInformation("Product {ProductId} deleted successfully", id);
            
            TempData["SuccessMessage"] = "Product was deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product {ProductId}", id);
            
            TempData["ErrorMessage"] = "An error occurred while deleting the product. Please try again.";
            return RedirectToAction(nameof(Index));
        }
    }
}