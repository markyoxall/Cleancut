using AutoMapper;
using CleanCut.Domain.Entities;
using CleanCut.WebApp.Models.Countries;
using CleanCut.WebApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CleanCut.WebApp.Controllers;

/// <summary>
/// MVC Controller for Product management via API calls
/// </summary>
[Authorize] // ? Require authentication for all actions
public class countriesController : Controller
{
    private readonly ICountryApiService _countryApiService;
    private readonly IMapper _mapper;
    private readonly ILogger<countriesController> _logger;

    public countriesController(
        ICountryApiService countryApiService,
        IMapper mapper,
        ILogger<countriesController> logger)
    {
        _countryApiService = countryApiService ?? throw new ArgumentNullException(nameof(countryApiService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Display list of countries
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index( string? searchTerm, bool? isAvailableFilter)
    {
        try
        {
            _logger.LogInformation("Loading countries list with customer filter: {CustomerId}, search: {SearchTerm}, available: {IsAvailableFilter}",
                 searchTerm, isAvailableFilter);

            // Load countries
           var  countries = await _countryApiService.GetAllCountriesAsync();


            // Apply filters
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                countries = countries.Where(p =>
                    p.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    p.Code.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
 

            var viewModel = new CountryListViewModel
            {
                Countries = countries.ToList(),
                SearchTerm = searchTerm,
                TotalCountries = countries.Count
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
            _logger.LogError(ex, "Error loading countries list");

            var errorViewModel = new CountryListViewModel
            {
                Message = "An error occurred while loading countries. Please try again.",
                IsSuccess = false
            };

            return View(errorViewModel);
        }
    }

    ///// <summary>
    ///// Display country details
    ///// </summary>
    //[HttpGet]
    //public async Task<IActionResult> Details(Guid id)
    //{
    //    try
    //    {
    //        _logger.LogInformation("Loading details for country {ProductId}", id);

    //        var country = await _countryApiService.GetProductByIdAsync(id);
    //        if (country == null)
    //        {
    //            _logger.LogWarning("Product {ProductId} not found", id);
    //            return NotFound("Product not found");
    //        }

    //        var owner = country.Customer ?? await _customerApiService.GetCustomerByIdAsync(country.CustomerId);

    //        var viewModel = new ProductDetailsViewModel
    //        {
    //            Product = country,
    //            Owner = owner
    //        };

    //        return View(viewModel);
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Error loading country details for {ProductId}", id);
    //        return BadRequest("An error occurred while loading country details");
    //    }
    //}

    ///// <summary>
    ///// Display create country form
    ///// </summary>
    //[HttpGet]
    //public async Task<IActionResult> Create()
    //{
    //    try
    //    {
    //        var customers = await _customerApiService.GetAllCustomersAsync();

    //        var viewModel = new ProductEditViewModel
    //        {
    //            AvailableCustomers = customers.ToList()
    //        };

    //        return View(viewModel);
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Error loading create country form");
    //        return BadRequest("An error occurred while loading the form");
    //    }
    //}

    ///// <summary>
    ///// Handle create country form submission
    ///// </summary>
    //[HttpPost]
    //[ValidateAntiForgeryToken]
    //public async Task<IActionResult> Create(ProductEditViewModel model)
    //{
    //    if (!ModelState.IsValid)
    //    {
    //        // Reload customers for dropdown
    //        var customers = await _customerApiService.GetAllCustomersAsync();
    //        model.AvailableCustomers = customers.ToList();
    //        return View(model);
    //    }

    //    try
    //    {
    //        _logger.LogInformation("Creating new country {ProductName} for customer {CustomerId}", model.Name, model.CustomerId);

    //        var createdProduct = await _countryApiService.CreateProductAsync(model.Name, model.Description, model.Price, model.CustomerId);

    //        _logger.LogInformation("Product created successfully with ID {ProductId}", createdProduct.Id);

    //        TempData["SuccessMessage"] = $"Product '{createdProduct.Name}' was created successfully.";
    //        return RedirectToAction(nameof(Index));
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Error creating country {ProductName}", model.Name);

    //        ModelState.AddModelError(string.Empty, "An error occurred while creating the country. Please try again.");

    //        // Reload customers for dropdown
    //        var customers = await _customerApiService.GetAllCustomersAsync();
    //        model.AvailableCustomers = customers.ToList();
    //        return View(model);
    //    }
    //}

    ///// <summary>
    ///// Display edit country form
    ///// </summary>
    //[HttpGet]
    //public async Task<IActionResult> Edit(Guid id)
    //{
    //    try
    //    {
    //        _logger.LogInformation("Loading country {ProductId} for editing", id);

    //        var country = await _countryApiService.GetProductByIdAsync(id);
    //        if (country == null)
    //        {
    //            _logger.LogWarning("Product {ProductId} not found for editing", id);
    //            return NotFound("Product not found");
    //        }

    //        var customers = await _customerApiService.GetAllCustomersAsync();

    //        var viewModel = _mapper.Map<ProductEditViewModel>(country);
    //        viewModel.AvailableCustomers = customers.ToList();

    //        return View(viewModel);
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Error loading country {ProductId} for editing", id);
    //        return BadRequest("An error occurred while loading the country for editing");
    //    }
    //}

    ///// <summary>
    ///// Handle edit country form submission
    ///// </summary>
    //[HttpPost]
    //[ValidateAntiForgeryToken]
    //public async Task<IActionResult> Edit(Guid id, ProductEditViewModel model)
    //{
    //    if (id != model.Id)
    //    {
    //        return BadRequest("ID mismatch");
    //    }

    //    if (!ModelState.IsValid)
    //    {
    //        // Reload customers for dropdown
    //        var customers = await _customerApiService.GetAllCustomersAsync();
    //        model.AvailableCustomers = customers.ToList();
    //        return View(model);
    //    }

    //    try
    //    {
    //        _logger.LogInformation("Updating country {ProductId}", id);

    //        var updatedProduct = await _countryApiService.UpdateProductAsync(id, model.Name, model.Description, model.Price);

    //        _logger.LogInformation("Product {ProductId} updated successfully", id);

    //        TempData["SuccessMessage"] = $"Product '{updatedProduct.Name}' was updated successfully.";
    //        return RedirectToAction(nameof(Index));
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Error updating country {ProductId}", id);

    //        ModelState.AddModelError(string.Empty, "An error occurred while updating the country. Please try again.");

    //        // Reload customers for dropdown
    //        var customers = await _customerApiService.GetAllCustomersAsync();
    //        model.AvailableCustomers = customers.ToList();
    //        return View(model);
    //    }
    //}

    ///// <summary>
    ///// Display delete confirmation
    ///// </summary>
    //[HttpGet]
    //public async Task<IActionResult> Delete(Guid id)
    //{
    //    try
    //    {
    //        var country = await _countryApiService.GetProductByIdAsync(id);
    //        if (country == null)
    //        {
    //            return NotFound("Product not found");
    //        }

    //        return View(country);
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Error loading country {ProductId} for deletion", id);
    //        return BadRequest("An error occurred while loading the country");
    //    }
    //}

    ///// <summary>
    ///// Handle delete confirmation
    ///// </summary>
    //[HttpPost, ActionName("Delete")]
    //[ValidateAntiForgeryToken]
    //public async Task<IActionResult> DeleteConfirmed(Guid id)
    //{
    //    try
    //    {
    //        var success = await _countryApiService.DeleteProductAsync(id);
    //        if (!success)
    //        {
    //            return NotFound("Product not found");
    //        }

    //        _logger.LogInformation("Product {ProductId} deleted successfully", id);

    //        TempData["SuccessMessage"] = "Product was deleted successfully.";
    //        return RedirectToAction(nameof(Index));
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Error deleting country {ProductId}", id);

    //        TempData["ErrorMessage"] = "An error occurred while deleting the country. Please try again.";
    //        return RedirectToAction(nameof(Index));
    //    }
    //}
}