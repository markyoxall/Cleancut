using AutoMapper;
using CleanCut.WebApp.Models.Customers;
using CleanCut.WebApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace CleanCut.WebApp.Controllers;

/// <summary>
/// MVC Controller for Customer management via API calls (renamed from CustomersController)
/// </summary>
[Authorize] // ? Require authentication for all actions
public class CustomersController : Controller
{
    private readonly ICustomerApiService _customerApiService;
    private readonly IProductApiService _productApiService;
    private readonly IMapper _mapper;
    private readonly ILogger<CustomersController> _logger;

    public CustomersController(
        ICustomerApiService customerApiService, 
        IProductApiService productApiService,
        IMapper mapper, 
        ILogger<CustomersController> logger)
    {
        _customerApiService = customerApiService ?? throw new ArgumentNullException(nameof(customerApiService));
        _productApiService = productApiService ?? throw new ArgumentNullException(nameof(productApiService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Display list of customers
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index(string? searchTerm, bool? isActiveFilter)
    {
        var requestId = Guid.NewGuid().ToString("N")[..8];
        try
        {
            _logger.LogInformation("CustomersController.Index called with search term: {SearchTerm}, active filter: {IsActiveFilter}", 
                requestId, searchTerm, isActiveFilter);

            var customers = await _customerApiService.GetAllCustomersAsync();
            
            // Apply filters
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                customers = customers.Where(u => 
                    u.FirstName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    u.LastName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    u.Email.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            if (isActiveFilter.HasValue)
            {
                customers = customers.Where(u => u.IsActive == isActiveFilter.Value).ToList();
            }

            var viewModel = new CustomerListViewModel
            {
                Customers = customers.ToList(),
                SearchTerm = searchTerm,
                IsActiveFilter = isActiveFilter,
                TotalCustomers = customers.Count()
            };

            // Check for success message in TempData
            if (TempData.ContainsKey("SuccessMessage"))
            {
                viewModel.Message = TempData["SuccessMessage"]?.ToString() ?? string.Empty;
                viewModel.IsSuccess = true;
            }

            _logger.LogInformation("CustomersController.Index returning {CustomerCount} customers", customers.Count());
            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in CustomersController.Index");
            
            var errorViewModel = new CustomerListViewModel
            {
                Message = $"An error occurred while loading customers: {ex.Message}",
                IsSuccess = false
            };
            
            return View(errorViewModel);
        }
    }

    /// <summary>
    /// Display customer details
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Details(Guid id)
    {
        var requestId = Guid.NewGuid().ToString("N")[..8];
        try
        {
            _logger.LogInformation("CustomersController.Details called for customer {Id}", requestId, id);

            var customer = await _customerApiService.GetCustomerByIdAsync(id);
            if (customer == null)
            {
                _logger.LogWarning("Customer {Id} not found in CustomersController.Details", requestId, id);
                return NotFound("Customer not found");
            }

            var customerProducts = await _productApiService.GetProductsByCustomerAsync(id);

            var viewModel = new CustomerDetailsViewModel
            {
                Customer = customer,
                CustomerProducts = customerProducts.ToList(),
                TotalProducts = customerProducts.Count()
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in CustomersController.Details for customer {Id}", requestId, id);
            return BadRequest("An error occurred while loading customer details");
        }
    }

    /// <summary>
    /// Display create Customer form
    /// </summary>
    [HttpGet]
    public IActionResult Create()
    {
        var requestId = Guid.NewGuid().ToString("N")[..8];
        _logger.LogInformation("?? [{RequestId}] CustomersController.Create (GET) called", requestId);
        var viewModel = new CustomerEditViewModel();
        return View(viewModel);
    }

    /// <summary>
    /// Handle create Customer form submission
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CustomerEditViewModel model)
    {
        var requestId = Guid.NewGuid().ToString("N")[..8];
        _logger.LogInformation("?? [{RequestId}] CustomersController.Create (POST) called for email {Email}", requestId, model.Email);
        
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var createdCustomer = await _customerApiService.CreateCustomerAsync(model.FirstName, model.LastName, model.Email);

            _logger.LogInformation("? [{RequestId}] Customer created successfully with ID {CustomerId}", requestId, createdCustomer.Id);
            
            TempData["SuccessMessage"] = $"Customer '{createdCustomer.FirstName} {createdCustomer.LastName}' was created successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? [{RequestId}] Error creating Customer with email {Email}", requestId, model.Email);
            
            ModelState.AddModelError(string.Empty, "An error occurred while creating the Customer. Please try again.");
            return View(model);
        }
    }

    /// <summary>
    /// Display edit Customer form
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var requestId = Guid.NewGuid().ToString("N")[..8];
        try
        {
            _logger.LogInformation("?? [{RequestId}] CustomersController.Edit (GET) called for Customer {CustomerId} | Thread: {ThreadId} | Time: {Time}", 
                requestId, id, Thread.CurrentThread.ManagedThreadId, DateTime.Now.ToString("HH:mm:ss.fff"));

            // Remove duplicate check since we know it's Redis causing delays
            _logger.LogInformation("?? [{RequestId}] Calling API to get Customer...", requestId);
            var Customer = await _customerApiService.GetCustomerByIdAsync(id);
            if (Customer == null)
            {
                _logger.LogWarning("? [{RequestId}] Customer {CustomerId} not found for editing", requestId, id);
                return NotFound("Customer not found");
            }

            _logger.LogInformation("??? [{RequestId}] Mapping Customer to view model...", requestId);
            var viewModel = _mapper.Map<CustomerEditViewModel>(Customer);
            
            _logger.LogInformation("? [{RequestId}] CustomersController.Edit returning view for Customer {CustomerId} | Thread: {ThreadId} | Model: {ModelData}", 
                requestId, id, Thread.CurrentThread.ManagedThreadId, 
                $"Id={viewModel.Id}, Name={viewModel.FirstName} {viewModel.LastName}");
            
            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? [{RequestId}] Error loading Customer {CustomerId} for editing", requestId, id);
            return BadRequest("An error occurred while loading the Customer for editing");
        }
    }

    /// <summary>
    /// Handle edit Customer form submission
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, CustomerEditViewModel model)
    {
        var requestId = Guid.NewGuid().ToString("N")[..8];
        _logger.LogInformation("?? [{RequestId}] CustomersController.Edit (POST) called for Customer {CustomerId}", requestId, id);
        
        if (id != model.Id)
        {
            return BadRequest("ID mismatch");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var updatedCustomer = await _customerApiService.UpdateCustomerAsync(id, model.FirstName, model.LastName, model.Email);

            _logger.LogInformation("? [{RequestId}] Customer {CustomerId} updated successfully", requestId, id);
            
            TempData["SuccessMessage"] = $"Customer '{updatedCustomer.FirstName} {updatedCustomer.LastName}' was updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? [{RequestId}] Error updating Customer {CustomerId}", requestId, id);
            
            ModelState.AddModelError(string.Empty, "An error occurred while updating the Customer. Please try again.");
            return View(model);
        }
    }

    /// <summary>
    /// Display delete confirmation
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Delete(Guid id)
    {
        var requestId = Guid.NewGuid().ToString("N")[..8];
        try
        {
            _logger.LogInformation("?? [{RequestId}] CustomersController.Delete (GET) called for Customer {CustomerId} | Thread: {ThreadId} | Time: {Time}", 
                requestId, id, Thread.CurrentThread.ManagedThreadId, DateTime.Now.ToString("HH:mm:ss.fff"));
            
            _logger.LogInformation("?? [{RequestId}] Calling API to get Customer...", requestId);
            var Customer = await _customerApiService.GetCustomerByIdAsync(id);
            if (Customer == null)
            {
                _logger.LogWarning("? [{RequestId}] Customer {CustomerId} not found for deletion", requestId, id);
                return NotFound("Customer not found");
            }

            _logger.LogInformation("? [{RequestId}] CustomersController.Delete returning view for Customer {CustomerId} | Customer: {CustomerName}", 
                requestId, id, $"{Customer.FirstName} {Customer.LastName}");
            return View(Customer);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? [{RequestId}] Error loading Customer {CustomerId} for deletion", requestId, id);
            return BadRequest("An error occurred while loading the Customer");
        }
    }

    /// <summary>
    /// Handle delete confirmation
    /// </summary>
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var requestId = Guid.NewGuid().ToString("N")[..8];
        try
        {
            _logger.LogInformation("?? [{RequestId}] CustomersController.DeleteConfirmed called for Customer {CustomerId}", requestId, id);
            
            var success = await _customerApiService.DeleteCustomerAsync(id);
            if (!success)
            {
                return NotFound("Customer not found");
            }

            _logger.LogInformation("? [{RequestId] Customer {CustomerId} deleted successfully", requestId, id);
            
            TempData["SuccessMessage"] = "Customer was deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? [{RequestId}] Error deleting Customer {CustomerId}", requestId, id);
            
            TempData["ErrorMessage"] = "An error occurred while deleting the Customer. Please try again.";
            return RedirectToAction(nameof(Index));
        }
    }
}