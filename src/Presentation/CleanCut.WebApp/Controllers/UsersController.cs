using AutoMapper;
using CleanCut.WebApp.Models.Users;
using CleanCut.WebApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace CleanCut.WebApp.Controllers;

/// <summary>
/// MVC Controller for User management via API calls
/// </summary>
public class UsersController : Controller
{
    private readonly IUserApiService _userApiService;
    private readonly IProductApiService _productApiService;
    private readonly IMapper _mapper;
    private readonly ILogger<UsersController> _logger;

    public UsersController(
        IUserApiService userApiService, 
        IProductApiService productApiService,
        IMapper mapper, 
        ILogger<UsersController> logger)
    {
        _userApiService = userApiService ?? throw new ArgumentNullException(nameof(userApiService));
        _productApiService = productApiService ?? throw new ArgumentNullException(nameof(productApiService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Display list of users
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index(string? searchTerm, bool? isActiveFilter)
    {
        var requestId = Guid.NewGuid().ToString("N")[..8];
        try
        {
            _logger.LogInformation("?? [{RequestId}] UsersController.Index called with search term: {SearchTerm}, active filter: {IsActiveFilter}", 
                requestId, searchTerm, isActiveFilter);

            var users = await _userApiService.GetAllUsersAsync();
            
            // Apply filters
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                users = users.Where(u => 
                    u.FirstName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    u.LastName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    u.Email.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            if (isActiveFilter.HasValue)
            {
                users = users.Where(u => u.IsActive == isActiveFilter.Value).ToList();
            }

            var viewModel = new UserListViewModel
            {
                Users = users.ToList(),
                SearchTerm = searchTerm,
                IsActiveFilter = isActiveFilter,
                TotalUsers = users.Count()
            };

            // Check for success message in TempData
            if (TempData.ContainsKey("SuccessMessage"))
            {
                viewModel.Message = TempData["SuccessMessage"]?.ToString() ?? string.Empty;
                viewModel.IsSuccess = true;
            }

            _logger.LogInformation("? [{RequestId}] UsersController.Index returning {UserCount} users", requestId, users.Count());
            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? [{RequestId}] Error in UsersController.Index", requestId);
            
            var errorViewModel = new UserListViewModel
            {
                Message = $"An error occurred while loading users: {ex.Message}",
                IsSuccess = false
            };
            
            return View(errorViewModel);
        }
    }

    /// <summary>
    /// Display user details
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Details(Guid id)
    {
        var requestId = Guid.NewGuid().ToString("N")[..8];
        try
        {
            _logger.LogInformation("?? [{RequestId}] UsersController.Details called for user {UserId} | Thread: {ThreadId} | Time: {Time}", 
                requestId, id, Thread.CurrentThread.ManagedThreadId, DateTime.Now.ToString("HH:mm:ss.fff"));

            _logger.LogInformation("?? [{RequestId}] Calling API to get user...", requestId);
            var user = await _userApiService.GetUserByIdAsync(id);
            if (user == null)
            {
                _logger.LogWarning("? [{RequestId}] User {UserId} not found in UsersController.Details", requestId, id);
                return NotFound("User not found");
            }

            _logger.LogInformation("?? [{RequestId}] Calling API to get user products...", requestId);
            var userProducts = await _productApiService.GetProductsByUserAsync(id);

            _logger.LogInformation("??? [{RequestId}] Creating view model...", requestId);
            var viewModel = new UserDetailsViewModel
            {
                User = user,
                UserProducts = userProducts.ToList(),
                TotalProducts = userProducts.Count()
            };

            _logger.LogInformation("? [{RequestId}] UsersController.Details returning view for user {UserId} | Thread: {ThreadId} | Products: {ProductCount}", 
                requestId, id, Thread.CurrentThread.ManagedThreadId, viewModel.TotalProducts);
            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? [{RequestId}] Error in UsersController.Details for user {UserId}", requestId, id);
            return BadRequest("An error occurred while loading user details");
        }
    }

    /// <summary>
    /// Display create user form
    /// </summary>
    [HttpGet]
    public IActionResult Create()
    {
        var requestId = Guid.NewGuid().ToString("N")[..8];
        _logger.LogInformation("?? [{RequestId}] UsersController.Create (GET) called", requestId);
        var viewModel = new UserEditViewModel();
        return View(viewModel);
    }

    /// <summary>
    /// Handle create user form submission
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UserEditViewModel model)
    {
        var requestId = Guid.NewGuid().ToString("N")[..8];
        _logger.LogInformation("?? [{RequestId}] UsersController.Create (POST) called for email {Email}", requestId, model.Email);
        
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var createdUser = await _userApiService.CreateUserAsync(model.FirstName, model.LastName, model.Email);

            _logger.LogInformation("? [{RequestId}] User created successfully with ID {UserId}", requestId, createdUser.Id);
            
            TempData["SuccessMessage"] = $"User '{createdUser.FirstName} {createdUser.LastName}' was created successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? [{RequestId}] Error creating user with email {Email}", requestId, model.Email);
            
            ModelState.AddModelError(string.Empty, "An error occurred while creating the user. Please try again.");
            return View(model);
        }
    }

    /// <summary>
    /// Display edit user form
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var requestId = Guid.NewGuid().ToString("N")[..8];
        try
        {
            _logger.LogInformation("?? [{RequestId}] UsersController.Edit (GET) called for user {UserId} | Thread: {ThreadId} | Time: {Time}", 
                requestId, id, Thread.CurrentThread.ManagedThreadId, DateTime.Now.ToString("HH:mm:ss.fff"));

            // Remove duplicate check since we know it's Redis causing delays
            _logger.LogInformation("?? [{RequestId}] Calling API to get user...", requestId);
            var user = await _userApiService.GetUserByIdAsync(id);
            if (user == null)
            {
                _logger.LogWarning("? [{RequestId}] User {UserId} not found for editing", requestId, id);
                return NotFound("User not found");
            }

            _logger.LogInformation("??? [{RequestId}] Mapping user to view model...", requestId);
            var viewModel = _mapper.Map<UserEditViewModel>(user);
            
            _logger.LogInformation("? [{RequestId}] UsersController.Edit returning view for user {UserId} | Thread: {ThreadId} | Model: {ModelData}", 
                requestId, id, Thread.CurrentThread.ManagedThreadId, 
                $"Id={viewModel.Id}, Name={viewModel.FirstName} {viewModel.LastName}");
            
            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? [{RequestId}] Error loading user {UserId} for editing", requestId, id);
            return BadRequest("An error occurred while loading the user for editing");
        }
    }

    /// <summary>
    /// Handle edit user form submission
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, UserEditViewModel model)
    {
        var requestId = Guid.NewGuid().ToString("N")[..8];
        _logger.LogInformation("?? [{RequestId}] UsersController.Edit (POST) called for user {UserId}", requestId, id);
        
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
            var updatedUser = await _userApiService.UpdateUserAsync(id, model.FirstName, model.LastName, model.Email);

            _logger.LogInformation("? [{RequestId}] User {UserId} updated successfully", requestId, id);
            
            TempData["SuccessMessage"] = $"User '{updatedUser.FirstName} {updatedUser.LastName}' was updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? [{RequestId}] Error updating user {UserId}", requestId, id);
            
            ModelState.AddModelError(string.Empty, "An error occurred while updating the user. Please try again.");
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
            _logger.LogInformation("?? [{RequestId}] UsersController.Delete (GET) called for user {UserId} | Thread: {ThreadId} | Time: {Time}", 
                requestId, id, Thread.CurrentThread.ManagedThreadId, DateTime.Now.ToString("HH:mm:ss.fff"));
            
            _logger.LogInformation("?? [{RequestId}] Calling API to get user...", requestId);
            var user = await _userApiService.GetUserByIdAsync(id);
            if (user == null)
            {
                _logger.LogWarning("? [{RequestId}] User {UserId} not found for deletion", requestId, id);
                return NotFound("User not found");
            }

            _logger.LogInformation("? [{RequestId}] UsersController.Delete returning view for user {UserId} | User: {UserName}", 
                requestId, id, $"{user.FirstName} {user.LastName}");
            return View(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? [{RequestId}] Error loading user {UserId} for deletion", requestId, id);
            return BadRequest("An error occurred while loading the user");
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
            _logger.LogInformation("?? [{RequestId}] UsersController.DeleteConfirmed called for user {UserId}", requestId, id);
            
            var success = await _userApiService.DeleteUserAsync(id);
            if (!success)
            {
                return NotFound("User not found");
            }

            _logger.LogInformation("? [{RequestId] User {UserId} deleted successfully", requestId, id);
            
            TempData["SuccessMessage"] = "User was deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? [{RequestId}] Error deleting user {UserId}", requestId, id);
            
            TempData["ErrorMessage"] = "An error occurred while deleting the user. Please try again.";
            return RedirectToAction(nameof(Index));
        }
    }
}