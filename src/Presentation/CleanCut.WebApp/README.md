# CleanCut.WebApp - Web Application Presentation Layer

## Purpose in Clean Architecture

The **Web Application Layer** provides a **browser-based user interface** for your application. It serves HTML pages, handles user interactions, and communicates with the Application Layer to execute business use cases. This layer focuses on user experience and web-specific concerns like navigation, forms, and client-side functionality.

## Key Principles

### 1. **User-Centric Design**
- Focus on user experience and usability
- Responsive design for different screen sizes
- Accessible and inclusive interface design

### 2. **MVC Pattern**
- Controllers handle user input and navigation
- Views render the user interface
- Models represent data for the view layer

### 3. **Client-Server Separation**
- Clean separation between server-side rendering and client-side behavior
- Progressive enhancement for better performance
- Graceful degradation for accessibility

### 4. **Web Standards Compliance**
- Semantic HTML markup
- CSS best practices
- Modern JavaScript features

## Folder Structure

```
CleanCut.WebApp/
??? Areas/               # Feature-based organization
?   ??? Admin/
?   ?   ??? Controllers/
?   ?   ??? Views/
?   ?   ??? Models/
?   ??? Customer/
?       ??? Controllers/
?       ??? Views/
?       ??? Models/
??? Controllers/         # Main application controllers
?   ??? HomeController.cs
?   ??? CustomerController.cs
?   ??? OrderController.cs
?   ??? AccountController.cs
??? Views/               # Razor view templates
?   ??? Shared/
?   ?   ??? _Layout.cshtml
?   ?   ??? _LoginPartial.cshtml
?   ?   ??? Error.cshtml
?   ??? Home/
?   ??? Customer/
?   ??? Order/
??? wwwroot/             # Static files
?   ??? css/
?   ??? js/
?   ??? images/
?   ??? lib/
??? ViewModels/          # View-specific models
?   ??? CustomerViewModel.cs
?   ??? OrderViewModel.cs
?   ??? DashboardViewModel.cs
??? Services/            # View-related services
?   ??? ViewModelService.cs
?   ??? NavigationService.cs
??? Extensions/          # Extension methods
?   ??? ControllerExtensions.cs
?   ??? ViewExtensions.cs
??? Configuration/       # Web app configuration
    ??? AutoMapperProfile.cs
    ??? ViewLocationExpander.cs
```

## What Goes Here

### Controllers
- Handle HTTP requests and user interactions
- Coordinate with Application Layer
- Prepare data for views
- Handle form submissions and navigation

### Views
- Razor templates for rendering HTML
- Layout pages and partial views
- Client-side templates and components

### ViewModels
- Data models specifically designed for views
- Aggregate data from multiple sources
- Handle view-specific logic and formatting

### Static Assets
- CSS stylesheets and frameworks
- JavaScript files and libraries
- Images, fonts, and other media

## Example Patterns

### MVC Controller
```csharp
public class CustomerController : Controller
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;
    private readonly ILogger<CustomerController> _logger;

    public CustomerController(IMediator mediator, IMapper mapper, ILogger<CustomerController> logger)
    {
        _mediator = mediator;
        _mapper = mapper;
        _logger = logger;
    }

    // GET: /Customer
    public async Task<IActionResult> Index(CustomerSearchViewModel searchModel)
    {
        var query = new SearchCustomersQuery
        {
            SearchTerm = searchModel.SearchTerm,
            PageNumber = searchModel.PageNumber,
            PageSize = searchModel.PageSize,
            SortBy = searchModel.SortBy,
            SortDirection = searchModel.SortDirection
        };

        var result = await _mediator.Send(query);
        
        var viewModel = new CustomerListViewModel
        {
            Customers = _mapper.Map<List<CustomerViewModel>>(result.Data),
            SearchModel = searchModel,
            TotalCount = result.TotalCount,
            TotalPages = result.TotalPages,
            HasNextPage = result.HasNextPage,
            HasPreviousPage = result.HasPreviousPage
        };

        return View(viewModel);
    }

    // GET: /Customer/Details/5
    public async Task<IActionResult> Details(Guid id)
    {
        var query = new GetCustomerQuery { CustomerId = id };
        var customer = await _mediator.Send(query);

        if (customer == null)
        {
            return NotFound();
        }

        var viewModel = _mapper.Map<CustomerDetailsViewModel>(customer);
        return View(viewModel);
    }

    // GET: /Customer/Create
    public IActionResult Create()
    {
        var viewModel = new CreateCustomerViewModel();
        return View(viewModel);
    }

    // POST: /Customer/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateCustomerViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        try
        {
            var command = _mapper.Map<CreateCustomerCommand>(viewModel);
            var result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = "Customer created successfully!";
                return RedirectToAction(nameof(Details), new { id = result.Value.Id });
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating customer");
            ModelState.AddModelError(string.Empty, "An error occurred while creating the customer.");
        }

        return View(viewModel);
    }

    // GET: /Customer/Edit/5
    public async Task<IActionResult> Edit(Guid id)
    {
        var query = new GetCustomerQuery { CustomerId = id };
        var customer = await _mediator.Send(query);

        if (customer == null)
        {
            return NotFound();
        }

        var viewModel = _mapper.Map<EditCustomerViewModel>(customer);
        return View(viewModel);
    }

    // POST: /Customer/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, EditCustomerViewModel viewModel)
    {
        if (id != viewModel.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        try
        {
            var command = _mapper.Map<UpdateCustomerCommand>(viewModel);
            var result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = "Customer updated successfully!";
                return RedirectToAction(nameof(Details), new { id = viewModel.Id });
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating customer {CustomerId}", id);
            ModelState.AddModelError(string.Empty, "An error occurred while updating the customer.");
        }

        return View(viewModel);
    }

    // POST: /Customer/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var command = new DeleteCustomerCommand { CustomerId = id };
            var result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = "Customer deleted successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = string.Join(", ", result.Errors);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting customer {CustomerId}", id);
            TempData["ErrorMessage"] = "An error occurred while deleting the customer.";
        }

        return RedirectToAction(nameof(Index));
    }
}
```

### ViewModel Classes
```csharp
public class CustomerListViewModel
{
    public List<CustomerViewModel> Customers { get; set; } = new();
    public CustomerSearchViewModel SearchModel { get; set; } = new();
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public bool HasNextPage { get; set; }
    public bool HasPreviousPage { get; set; }
}

public class CustomerViewModel
{
    public Guid Id { get; set; }
    
    [Display(Name = "Customer Name")]
    public string Name { get; set; }
    
    [Display(Name = "Email Address")]
    public string Email { get; set; }
    
    [Display(Name = "Phone Number")]
    public string Phone { get; set; }
    
    [Display(Name = "Registration Date")]
    [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
    public DateTime CreatedAt { get; set; }
    
    [Display(Name = "VIP Status")]
    public bool IsVip { get; set; }
    
    public AddressViewModel Address { get; set; }
}

public class CreateCustomerViewModel
{
    [Required(ErrorMessage = "Customer name is required")]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
    [Display(Name = "Customer Name")]
    public string Name { get; set; }

    [Required(ErrorMessage = "Email address is required")]
    [EmailAddress(ErrorMessage = "Invalid email address format")]
    [Display(Name = "Email Address")]
    public string Email { get; set; }

    [Phone(ErrorMessage = "Invalid phone number format")]
    [Display(Name = "Phone Number")]
    public string Phone { get; set; }

    [Display(Name = "VIP Customer")]
    public bool IsVip { get; set; }

    public AddressViewModel Address { get; set; } = new();
}

public class EditCustomerViewModel : CreateCustomerViewModel
{
    public Guid Id { get; set; }
    
    [Display(Name = "Registration Date")]
    [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
    public DateTime CreatedAt { get; set; }
}

public class CustomerSearchViewModel
{
    [Display(Name = "Search")]
    public string SearchTerm { get; set; }
    
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    
    [Display(Name = "Sort By")]
    public string SortBy { get; set; } = "Name";
    
    [Display(Name = "Sort Direction")]
    public string SortDirection { get; set; } = "asc";
}

public class AddressViewModel
{
    [Display(Name = "Street Address")]
    [StringLength(200)]
    public string Street { get; set; }

    [Display(Name = "City")]
    [StringLength(100)]
    public string City { get; set; }

    [Display(Name = "State/Province")]
    [StringLength(50)]
    public string State { get; set; }

    [Display(Name = "Postal Code")]
    [StringLength(20)]
    public string PostalCode { get; set; }

    [Display(Name = "Country")]
    [StringLength(50)]
    public string Country { get; set; }
}
```

### Razor Views
```html
@* Views/Customer/Index.cshtml *@
@model CustomerListViewModel

@{
    ViewData["Title"] = "Customers";
}

<div class="container-fluid">
    <div class="row">
        <div class="col-12">
            <div class="d-flex justify-content-between align-items-center mb-4">
                <h2>@ViewData["Title"]</h2>
                <a asp-action="Create" class="btn btn-primary">
                    <i class="fas fa-plus"></i> Add New Customer
                </a>
            </div>

            @* Search Form *@
            <div class="card mb-4">
                <div class="card-body">
                    <form asp-action="Index" method="get">
                        <div class="row">
                            <div class="col-md-4">
                                <div class="form-group">
                                    <label asp-for="SearchModel.SearchTerm"></label>
                                    <input asp-for="SearchModel.SearchTerm" class="form-control" placeholder="Search customers..." />
                                </div>
                            </div>
                            <div class="col-md-2">
                                <div class="form-group">
                                    <label asp-for="SearchModel.SortBy"></label>
                                    <select asp-for="SearchModel.SortBy" class="form-control">
                                        <option value="Name">Name</option>
                                        <option value="Email">Email</option>
                                        <option value="CreatedAt">Date Created</option>
                                    </select>
                                </div>
                            </div>
                            <div class="col-md-2">
                                <div class="form-group">
                                    <label asp-for="SearchModel.SortDirection"></label>
                                    <select asp-for="SearchModel.SortDirection" class="form-control">
                                        <option value="asc">Ascending</option>
                                        <option value="desc">Descending</option>
                                    </select>
                                </div>
                            </div>
                            <div class="col-md-2">
                                <div class="form-group">
                                    <label asp-for="SearchModel.PageSize"></label>
                                    <select asp-for="SearchModel.PageSize" class="form-control">
                                        <option value="10">10</option>
                                        <option value="25">25</option>
                                        <option value="50">50</option>
                                    </select>
                                </div>
                            </div>
                            <div class="col-md-2">
                                <div class="form-group">
                                    <label>&nbsp;</label>
                                    <div>
                                        <button type="submit" class="btn btn-primary">
                                            <i class="fas fa-search"></i> Search
                                        </button>
                                        <a asp-action="Index" class="btn btn-secondary">Clear</a>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </form>
                </div>
            </div>

            @* Results Table *@
            <div class="card">
                <div class="card-body">
                    @if (Model.Customers.Any())
                    {
                        <div class="table-responsive">
                            <table class="table table-striped table-hover">
                                <thead class="thead-dark">
                                    <tr>
                                        <th>@Html.DisplayNameFor(m => m.Customers.First().Name)</th>
                                        <th>@Html.DisplayNameFor(m => m.Customers.First().Email)</th>
                                        <th>@Html.DisplayNameFor(m => m.Customers.First().Phone)</th>
                                        <th>@Html.DisplayNameFor(m => m.Customers.First().CreatedAt)</th>
                                        <th>@Html.DisplayNameFor(m => m.Customers.First().IsVip)</th>
                                        <th>Actions</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    @foreach (var customer in Model.Customers)
                                    {
                                        <tr>
                                            <td>@customer.Name</td>
                                            <td>@customer.Email</td>
                                            <td>@customer.Phone</td>
                                            <td>@Html.DisplayFor(m => customer.CreatedAt)</td>
                                            <td>
                                                @if (customer.IsVip)
                                                {
                                                    <span class="badge badge-success">VIP</span>
                                                }
                                                else
                                                {
                                                    <span class="badge badge-secondary">Regular</span>
                                                }
                                            </td>
                                            <td>
                                                <div class="btn-group" role="group">
                                                    <a asp-action="Details" asp-route-id="@customer.Id" class="btn btn-sm btn-info">
                                                        <i class="fas fa-eye"></i>
                                                    </a>
                                                    <a asp-action="Edit" asp-route-id="@customer.Id" class="btn btn-sm btn-warning">
                                                        <i class="fas fa-edit"></i>
                                                    </a>
                                                    <button type="button" class="btn btn-sm btn-danger" 
                                                            onclick="confirmDelete('@customer.Id', '@customer.Name')">
                                                        <i class="fas fa-trash"></i>
                                                    </button>
                                                </div>
                                            </td>
                                        </tr>
                                    }
                                </tbody>
                            </table>
                        </div>

                        @* Pagination *@
                        @if (Model.TotalPages > 1)
                        {
                            <nav aria-label="Customer pagination">
                                <ul class="pagination justify-content-center">
                                    @if (Model.HasPreviousPage)
                                    {
                                        <li class="page-item">
                                            <a class="page-link" asp-action="Index" 
                                               asp-route-pageNumber="@(Model.SearchModel.PageNumber - 1)"
                                               asp-route-searchTerm="@Model.SearchModel.SearchTerm"
                                               asp-route-sortBy="@Model.SearchModel.SortBy"
                                               asp-route-sortDirection="@Model.SearchModel.SortDirection"
                                               asp-route-pageSize="@Model.SearchModel.PageSize">
                                                Previous
                                            </a>
                                        </li>
                                    }

                                    @for (int i = Math.Max(1, Model.SearchModel.PageNumber - 2); 
                                          i <= Math.Min(Model.TotalPages, Model.SearchModel.PageNumber + 2); i++)
                                    {
                                        <li class="page-item @(i == Model.SearchModel.PageNumber ? "active" : "")">
                                            <a class="page-link" asp-action="Index" 
                                               asp-route-pageNumber="@i"
                                               asp-route-searchTerm="@Model.SearchModel.SearchTerm"
                                               asp-route-sortBy="@Model.SearchModel.SortBy"
                                               asp-route-sortDirection="@Model.SearchModel.SortDirection"
                                               asp-route-pageSize="@Model.SearchModel.PageSize">
                                                @i
                                            </a>
                                        </li>
                                    }

                                    @if (Model.HasNextPage)
                                    {
                                        <li class="page-item">
                                            <a class="page-link" asp-action="Index" 
                                               asp-route-pageNumber="@(Model.SearchModel.PageNumber + 1)"
                                               asp-route-searchTerm="@Model.SearchModel.SearchTerm"
                                               asp-route-sortBy="@Model.SearchModel.SortBy"
                                               asp-route-sortDirection="@Model.SearchModel.SortDirection"
                                               asp-route-pageSize="@Model.SearchModel.PageSize">
                                                Next
                                            </a>
                                        </li>
                                    }
                                </ul>
                            </nav>
                        }
                    }
                    else
                    {
                        <div class="text-center py-4">
                            <i class="fas fa-users fa-3x text-muted mb-3"></i>
                            <h4>No Customers Found</h4>
                            <p class="text-muted">Try adjusting your search criteria or add a new customer.</p>
                            <a asp-action="Create" class="btn btn-primary">Add First Customer</a>
                        </div>
                    }
                </div>
            </div>
        </div>
    </div>
</div>

@* Delete Confirmation Modal *@
<div class="modal fade" id="deleteModal" tabindex="-1">
    <div class="modal-dialog">
        <div class="modal-content">
            <div class="modal-header">
                <h5 class="modal-title">Confirm Delete</h5>
                <button type="button" class="close" data-dismiss="modal">
                    <span>&times;</span>
                </button>
            </div>
            <div class="modal-body">
                <p>Are you sure you want to delete <strong id="customerName"></strong>?</p>
                <p class="text-muted">This action cannot be undone.</p>
            </div>
            <div class="modal-footer">
                <button type="button" class="btn btn-secondary" data-dismiss="modal">Cancel</button>
                <form id="deleteForm" method="post" style="display: inline;">
                    @Html.AntiForgeryToken()
                    <button type="submit" class="btn btn-danger">Delete</button>
                </form>
            </div>
        </div>
    </div>
</div>

@section Scripts {
    <script>
        function confirmDelete(customerId, customerName) {
            document.getElementById('customerName').textContent = customerName;
            document.getElementById('deleteForm').action = '@Url.Action("Delete")/' + customerId;
            $('#deleteModal').modal('show');
        }
    </script>
}
```

### Layout Page
```html
@* Views/Shared/_Layout.cshtml *@
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>@ViewData["Title"] - CleanCut</title>
    
    <link rel="stylesheet" href="~/lib/bootstrap/dist/css/bootstrap.min.css" />
    <link rel="stylesheet" href="~/lib/fontawesome/css/all.min.css" />
    <link rel="stylesheet" href="~/css/site.css" asp-append-version="true" />
</head>
<body>
    <header>
        <nav class="navbar navbar-expand-sm navbar-toggleable-sm navbar-dark bg-dark border-bottom box-shadow mb-3">
            <div class="container">
                <a class="navbar-brand" asp-area="" asp-controller="Home" asp-action="Index">
                    <i class="fas fa-cut"></i> CleanCut
                </a>
                <button class="navbar-toggler" type="button" data-toggle="collapse" data-target=".navbar-collapse">
                    <span class="navbar-toggler-icon"></span>
                </button>
                <div class="navbar-collapse collapse d-sm-inline-flex justify-content-between">
                    <ul class="navbar-nav flex-grow-1">
                        <li class="nav-item">
                            <a class="nav-link" asp-area="" asp-controller="Home" asp-action="Index">Home</a>
                        </li>
                        <li class="nav-item">
                            <a class="nav-link" asp-area="" asp-controller="Customer" asp-action="Index">Customers</a>
                        </li>
                        <li class="nav-item">
                            <a class="nav-link" asp-area="" asp-controller="Order" asp-action="Index">Orders</a>
                        </li>
                        <li class="nav-item">
                            <a class="nav-link" asp-area="" asp-controller="Product" asp-action="Index">Products</a>
                        </li>
                    </ul>
                    <partial name="_LoginPartial" />
                </div>
            </div>
        </nav>
    </header>
    
    <div class="container">
        @* Alert Messages *@
        @if (TempData["SuccessMessage"] != null)
        {
            <div class="alert alert-success alert-dismissible fade show" role="alert">
                <i class="fas fa-check-circle"></i> @TempData["SuccessMessage"]
                <button type="button" class="close" data-dismiss="alert">
                    <span>&times;</span>
                </button>
            </div>
        }
        
        @if (TempData["ErrorMessage"] != null)
        {
            <div class="alert alert-danger alert-dismissible fade show" role="alert">
                <i class="fas fa-exclamation-circle"></i> @TempData["ErrorMessage"]
                <button type="button" class="close" data-dismiss="alert">
                    <span>&times;</span>
                </button>
            </div>
        }
        
        <main role="main" class="pb-3">
            @RenderBody()
        </main>
    </div>

    <footer class="border-top footer text-muted">
        <div class="container">
            &copy; @DateTime.Now.Year - CleanCut - <a asp-area="" asp-controller="Home" asp-action="Privacy">Privacy</a>
        </div>
    </footer>

    <script src="~/lib/jquery/dist/jquery.min.js"></script>
    <script src="~/lib/bootstrap/dist/js/bootstrap.bundle.min.js"></script>
    <script src="~/js/site.js" asp-append-version="true"></script>
    
    @await RenderSectionAsync("Scripts", required: false)
</body>
</html>
```

## Key Technologies & Packages

### Required NuGet Packages
```xml
<PackageReference Include="Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation" Version="8.0.0" />
<PackageReference Include="Microsoft.AspNetCore.Identity.UI" Version="8.0.0" />
<PackageReference Include="AutoMapper.Extensions.Microsoft.DependencyInjection" Version="12.0.1" />
<PackageReference Include="MediatR" Version="12.2.0" />
<PackageReference Include="FluentValidation.AspNetCore" Version="11.3.0" />
<PackageReference Include="BuildBundlerMinifier" Version="3.2.449" />
```

### Project References
- **CleanCut.Application** (for commands, queries, and DTOs)
- **CleanCut.Infrastructure.Identity** (for authentication)
- **CleanCut.Infrastructure.Shared** (for shared services)

## Configuration & Startup

### Program.cs
```csharp
var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllersWithViews()
    .AddRazorRuntimeCompilation(); // For development

// Add Identity
builder.Services.AddDefaultIdentity<ApplicationUser>(options => 
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 6;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<IdentityDbContext>();

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(ViewModelMappingProfile));

// Add MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetCustomerQuery).Assembly));

// Add application services
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

var app = builder.Build();

// Configure pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Configure routes
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
```

## Testing Strategy

### Unit Tests
- Test controller actions with mocked dependencies
- Verify view model mapping
- Test custom action filters and attributes

### Integration Tests
- Test complete user workflows
- Verify form submissions and validation
- Test authentication and authorization

### UI Tests
- Selenium tests for critical user journeys
- Accessibility testing
- Cross-browser compatibility testing

## Common Patterns

### Base Controller
```csharp
public abstract class BaseController : Controller
{
    protected void ShowSuccessMessage(string message)
    {
        TempData["SuccessMessage"] = message;
    }

    protected void ShowErrorMessage(string message)
    {
        TempData["ErrorMessage"] = message;
    }

    protected void AddModelErrors(IEnumerable<string> errors)
    {
        foreach (var error in errors)
        {
            ModelState.AddModelError(string.Empty, error);
        }
    }

    protected string GetUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }
}
```

### Auto Mapper Profile
```csharp
public class ViewModelMappingProfile : Profile
{
    public ViewModelMappingProfile()
    {
        // Customer mappings
        CreateMap<CustomerDto, CustomerViewModel>();
        CreateMap<CustomerDto, CustomerDetailsViewModel>();
        CreateMap<CustomerDto, EditCustomerViewModel>();
        CreateMap<CreateCustomerViewModel, CreateCustomerCommand>();
        CreateMap<EditCustomerViewModel, UpdateCustomerCommand>();

        // Address mappings
        CreateMap<AddressDto, AddressViewModel>();
        CreateMap<AddressViewModel, AddressDto>();
    }
}
```

## Common Mistakes to Avoid

? **Fat Controllers** - Don't put business logic in controllers
? **ViewBag/ViewData Overuse** - Use strongly-typed ViewModels instead
? **Missing Validation** - Always validate user input
? **Poor UX** - Don't ignore user experience design
? **Inconsistent UI** - Maintain consistent design patterns

? **Thin Controllers** - Delegate to Application Layer
? **Strongly-Typed Views** - Use ViewModels for type safety
? **Responsive Design** - Support multiple screen sizes
? **Accessibility** - Follow WCAG guidelines
? **Progressive Enhancement** - Ensure functionality without JavaScript

This layer is your user's primary interaction point - make it intuitive, responsive, and accessible!