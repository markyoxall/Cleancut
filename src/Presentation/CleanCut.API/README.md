# CleanCut.API - Web API Presentation Layer

## Purpose in Clean Architecture

The **API Layer** serves as the **HTTP/REST interface** for your application. It handles web requests, coordinates with the Application Layer to execute business use cases, and returns appropriate responses. This layer translates HTTP concerns into application commands/queries and vice versa.

## Key Principles

### 1. **Thin Controllers**
- Controllers should be lightweight and focused on HTTP concerns
- Delegate business logic to the Application Layer
- Handle only routing, model binding, and response formatting

### 2. **RESTful Design**
- Follow REST conventions for URL structure and HTTP verbs
- Use appropriate HTTP status codes
- Implement consistent API patterns

### 3. **API-First Design**
- Design APIs with consumers in mind
- Provide clear documentation and contracts
- Support versioning for backward compatibility

### 4. **Cross-Cutting Concerns**
- Authentication and authorization
- Request/response logging
- Error handling and exception management
- Input validation and model binding

## Folder Structure

```
CleanCut.API/
??? Controllers/         # API controllers organized by domain
?   ??? CustomersController.cs
?   ??? OrdersController.cs
?   ??? ProductsController.cs
?   ??? AuthController.cs
??? Middleware/          # Custom middleware components
?   ??? ExceptionMiddleware.cs
?   ??? RequestLoggingMiddleware.cs
?   ??? PerformanceMiddleware.cs
??? Filters/             # Action filters and attributes
?   ??? ValidationFilter.cs
?   ??? AuthorizationFilter.cs
?   ??? CacheFilter.cs
??? Extensions/          # Extension methods for setup
?   ??? ServiceCollectionExtensions.cs
?   ??? ApplicationBuilderExtensions.cs
?   ??? ControllerExtensions.cs
??? Configuration/       # API-specific configuration
?   ??? SwaggerConfig.cs
?   ??? CorsConfig.cs
?   ??? ApiVersioningConfig.cs
??? Models/              # API-specific models (if needed)
?   ??? Requests/
?   ??? Responses/
?   ??? ApiResponse.cs
??? Validators/          # API input validators
    ??? CreateCustomerRequestValidator.cs
    ??? UpdateOrderRequestValidator.cs
```

## What Goes Here

### Controllers
- Handle HTTP requests and responses
- Route requests to appropriate Application Layer handlers
- Return consistent API responses
- Handle authentication and authorization

### Middleware
- Cross-cutting concerns that run for every request
- Exception handling, logging, performance monitoring
- Authentication, CORS, request validation

### API Models
- Request/Response DTOs specific to the API
- Different from Application Layer DTOs (API versioning)
- Input validation models

### Filters & Attributes
- Reusable logic that can be applied to controllers/actions
- Validation, caching, authorization, logging

## Example Patterns

### RESTful Controller
```csharp
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize]
public class CustomersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<CustomersController> _logger;

    public CustomersController(IMediator mediator, ILogger<CustomersController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get customer by ID
    /// </summary>
    /// <param name="id">Customer ID</param>
    /// <returns>Customer details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CustomerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CustomerResponse>> GetCustomer(Guid id)
    {
        try
        {
            var query = new GetCustomerQuery { CustomerId = id };
            var customer = await _mediator.Send(query);
            
            if (customer == null)
                return NotFound(new ApiError("Customer not found"));

            return Ok(customer);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customer {CustomerId}", id);
            return StatusCode(500, new ApiError("Internal server error"));
        }
    }

    /// <summary>
    /// Create a new customer
    /// </summary>
    /// <param name="request">Customer creation request</param>
    /// <returns>Created customer</returns>
    [HttpPost]
    [ProducesResponseType(typeof(CustomerResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<CustomerResponse>> CreateCustomer([FromBody] CreateCustomerRequest request)
    {
        var command = new CreateCustomerCommand
        {
            Name = request.Name,
            Email = request.Email,
            Address = request.Address
        };

        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
            return BadRequest(new ApiError(result.Errors));

        var customer = result.Value;
        return CreatedAtAction(nameof(GetCustomer), new { id = customer.Id }, customer);
    }

    /// <summary>
    /// Update existing customer
    /// </summary>
    /// <param name="id">Customer ID</param>
    /// <param name="request">Customer update request</param>
    /// <returns>Updated customer</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(CustomerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CustomerResponse>> UpdateCustomer(Guid id, [FromBody] UpdateCustomerRequest request)
    {
        var command = new UpdateCustomerCommand
        {
            CustomerId = id,
            Name = request.Name,
            Email = request.Email,
            Address = request.Address
        };

        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
        {
            if (result.Errors.Any(e => e.Contains("not found")))
                return NotFound(new ApiError("Customer not found"));
            
            return BadRequest(new ApiError(result.Errors));
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Delete customer
    /// </summary>
    /// <param name="id">Customer ID</param>
    /// <returns>No content</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteCustomer(Guid id)
    {
        var command = new DeleteCustomerCommand { CustomerId = id };
        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
        {
            if (result.Errors.Any(e => e.Contains("not found")))
                return NotFound(new ApiError("Customer not found"));
            
            return BadRequest(new ApiError(result.Errors));
        }

        return NoContent();
    }

    /// <summary>
    /// Search customers
    /// </summary>
    /// <param name="request">Search parameters</param>
    /// <returns>Paginated customer list</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<CustomerResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResponse<CustomerResponse>>> SearchCustomers([FromQuery] SearchCustomersRequest request)
    {
        var query = new SearchCustomersQuery
        {
            SearchTerm = request.SearchTerm,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            SortBy = request.SortBy,
            SortDirection = request.SortDirection
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }
}
```

### Exception Middleware
```csharp
public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IWebHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = new ApiError();

        switch (exception)
        {
            case ValidationException validationEx:
                response.Message = "Validation failed";
                response.Details = validationEx.Errors.Select(e => e.ErrorMessage).ToList();
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                break;

            case UnauthorizedAccessException:
                response.Message = "Unauthorized access";
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                break;

            case NotFoundException notFoundEx:
                response.Message = notFoundEx.Message;
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                break;

            case DomainException domainEx:
                response.Message = domainEx.Message;
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                break;

            default:
                response.Message = _environment.IsDevelopment() 
                    ? exception.Message 
                    : "An error occurred while processing your request";
                    
                if (_environment.IsDevelopment())
                {
                    response.Details = new List<string> { exception.StackTrace };
                }
                
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                break;
        }

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }
}
```

### API Response Models
```csharp
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T Data { get; set; }
    public string Message { get; set; }
    public List<string> Errors { get; set; } = new();

    public static ApiResponse<T> SuccessResult(T data, string message = null)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data,
            Message = message
        };
    }

    public static ApiResponse<T> ErrorResult(string message, List<string> errors = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Errors = errors ?? new List<string>()
        };
    }
}

public class ApiError
{
    public string Message { get; set; }
    public List<string> Details { get; set; } = new();
    public string TraceId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public ApiError(string message = null)
    {
        Message = message;
    }

    public ApiError(IEnumerable<string> errors)
    {
        Details = errors.ToList();
        Message = "One or more validation errors occurred";
    }
}

public class PagedResponse<T>
{
    public List<T> Data { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public bool HasNextPage { get; set; }
    public bool HasPreviousPage { get; set; }

    public PagedResponse(List<T> data, int pageNumber, int pageSize, int totalCount)
    {
        Data = data;
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalCount = totalCount;
        TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        HasNextPage = pageNumber < TotalPages;
        HasPreviousPage = pageNumber > 1;
    }
}
```

### Authentication Controller
```csharp
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class AuthController : ControllerBase
{
    private readonly IAuthenticationService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthenticationService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// User login
    /// </summary>
    /// <param name="request">Login credentials</param>
    /// <returns>Authentication result with token</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthenticationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        
        if (!result.IsSuccess)
        {
            return Unauthorized(new ApiError(result.ErrorMessage));
        }

        return Ok(new AuthenticationResponse
        {
            Token = result.Token,
            RefreshToken = result.RefreshToken,
            ExpiresAt = result.ExpiresAt,
            User = result.User
        });
    }

    /// <summary>
    /// User registration
    /// </summary>
    /// <param name="request">Registration details</param>
    /// <returns>Authentication result</returns>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthenticationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);
        
        if (!result.IsSuccess)
        {
            return BadRequest(new ApiError(result.ErrorMessage));
        }

        return CreatedAtAction(nameof(Login), new AuthenticationResponse
        {
            Token = result.Token,
            RefreshToken = result.RefreshToken,
            ExpiresAt = result.ExpiresAt,
            User = result.User
        });
    }

    /// <summary>
    /// Refresh access token
    /// </summary>
    /// <param name="request">Refresh token request</param>
    /// <returns>New access token</returns>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AuthenticationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
    {
        var result = await _authService.RefreshTokenAsync(request.RefreshToken);
        
        if (!result.IsSuccess)
        {
            return BadRequest(new ApiError(result.ErrorMessage));
        }

        return Ok(new AuthenticationResponse
        {
            Token = result.Token,
            RefreshToken = result.RefreshToken,
            ExpiresAt = result.ExpiresAt
        });
    }

    /// <summary>
    /// Logout user
    /// </summary>
    /// <returns>No content</returns>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        await _authService.LogoutAsync(userId);
        
        return NoContent();
    }
}
```

## Key Technologies & Packages

### Required NuGet Packages
```xml
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="8.0.0" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.0" />
<PackageReference Include="Microsoft.AspNetCore.Mvc.Versioning" Version="5.1.0" />
<PackageReference Include="Microsoft.AspNetCore.Mvc.Versioning.ApiExplorer" Version="5.1.0" />
<PackageReference Include="FluentValidation.AspNetCore" Version="11.3.0" />
<PackageReference Include="MediatR" Version="12.2.0" />
<PackageReference Include="Serilog.AspNetCore" Version="7.0.0" />
```

### Project References
- **CleanCut.Application** (for commands, queries, and DTOs)
- **CleanCut.Infrastructure.Identity** (for authentication services)
- **CleanCut.Infrastructure.Shared** (for shared services)

## Configuration & Startup

### Program.cs (.NET 10)
```csharp
var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers()
    .AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<CreateCustomerRequestValidator>());

// Add API versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new HeaderApiVersionReader("X-Version"),
        new QueryStringApiVersionReader("version"));
});

builder.Services.AddVersionedApiExplorer(setup =>
{
    setup.GroupNameFormat = "'v'VVV";
    setup.SubstituteApiVersionInUrl = true;
});

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "https://yourdomain.com")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Add authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // JWT configuration
    });

// Add application services
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// Add MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetCustomerQuery).Assembly));

var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "CleanCut API V1");
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowSpecificOrigins");

// Custom middleware
app.UseMiddleware<ExceptionMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
```

## API Documentation

### Swagger Configuration
```csharp
public static class SwaggerConfig
{
    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "CleanCut API",
                Version = "v1",
                Description = "A clean architecture API for CleanCut application",
                Contact = new OpenApiContact
                {
                    Name = "Development Team",
                    Email = "dev@cleancut.com"
                }
            });

            // Add JWT authentication
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });

            // Include XML comments
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            c.IncludeXmlComments(xmlPath);
        });

        return services;
    }
}
```

## Testing Strategy

### Integration Tests
- Test complete API endpoints with TestServer
- Verify authentication and authorization
- Test error handling and response formats

### Unit Tests
- Test controller logic with mocked dependencies
- Verify proper command/query mapping
- Test custom middleware and filters

## Common Patterns

### Base Controller
```csharp
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    protected IActionResult HandleResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
        {
            return result.Value != null ? Ok(result.Value) : NotFound();
        }

        return BadRequest(new ApiError(result.Errors));
    }

    protected string GetUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    protected bool IsUserInRole(string role)
    {
        return User.IsInRole(role);
    }
}
```

## Common Mistakes to Avoid

? **Fat Controllers** - Don't put business logic in controllers
? **Inconsistent Responses** - Use consistent response formats
? **Poor Error Handling** - Don't expose internal error details
? **Missing Validation** - Always validate input
? **Inconsistent HTTP Status Codes** - Use appropriate status codes

? **Thin Controllers** - Delegate to Application Layer
? **Consistent API Design** - Follow REST conventions
? **Proper Error Handling** - Use middleware for global error handling
? **API Documentation** - Maintain up-to-date Swagger documentation
? **Versioning Strategy** - Plan for API evolution

This layer is your application's front door - make it welcoming, secure, and well-documented!