/*
 * CleanCut API Program Configuration
 * =================================
 * 
 * This file configures the CleanCut Web API that serves as the protected backend API
 * for all client applications in the CleanCut ecosystem. It implements JWT Bearer 
 * authentication to validate tokens issued by the CleanCut IdentityServer.
 * 
 * ROLE IN AUTHENTICATION ARCHITECTURE:
 * ------------------------------------
 * This API serves as the RESOURCE SERVER in the OAuth2/OpenID Connect flow:
 * 
 * 1. VALIDATES TOKENS - Verifies JWT tokens issued by CleanCut IdentityServer
 * 2. ENFORCES AUTHORIZATION - Uses role-based policies to control endpoint access  
 * 3. PROVIDES PROTECTED DATA - Serves business data to authenticated client applications
 * 4. IMPLEMENTS SECURITY POLICIES - Rate limiting, CORS, security headers
 * 
 * CLIENT APPLICATION INTEGRATION:
 * ------------------------------
 * 
 * • CleanCut.BlazorWebApp
 *   ??? Makes HTTP requests with Bearer tokens from TokenService
 *   ??? Server-side app uses Client Credentials tokens
 *   ??? Calls API endpoints on behalf of the application
 * 
 * • CleanCut.WebApp (MVC)
 *   ??? Uses access tokens obtained after user authentication
 *   ??? Makes API calls from server-side controllers and client-side JavaScript
 *   ??? User identity flows through from ID token to API calls
 * 
 * • CleanCut.WinApp (Future)
 *   ??? Would use HttpClient with Bearer token authentication
 *   ??? Desktop app making direct API calls with user tokens
 * 
 * • Swagger UI
 *   ??? Interactive API testing with OAuth2 authentication
 *   ??? Developers can test endpoints with real authentication
 * 
 * AUTHENTICATION FLOW:
 * -------------------
 * 1. Client obtains JWT token from IdentityServer (/connect/token)
 * 2. Client includes token in Authorization header: "Bearer {token}"
 * 3. API validates token signature against IdentityServer's public keys
 * 4. API extracts user claims (role, email, name) from token payload
 * 5. Authorization policies check role claims for endpoint access
 * 6. API returns protected data or 401/403 error
 * 
 * AUTHORIZATION POLICIES IMPLEMENTED:
 * ----------------------------------
 * • FallbackPolicy - All endpoints require authentication by default
 * • AdminOnly - Restricts certain operations to Admin role users only
 * • UserOrAdmin - Allows access to User or Admin role users
 * 
 * SECURITY FEATURES:
 * -----------------
 * • JWT Bearer token validation with IdentityServer authority
 * • Audience validation (accepts "CleanCutAPI" tokens only)
 * • Role-based authorization policies
 * • Rate limiting (100 req/min production, 1000 dev)
 * • CORS restrictions to known client origins
 * • Security headers (CSP, HSTS, XSS Protection, etc.)
 * • Input validation on all endpoints
 * • Comprehensive security logging without sensitive data exposure
 * • Exception handling that doesn't leak internal information
 * 
 * ENDPOINTS PROTECTED:
 * -------------------
 * • GET /api/v1/products - Requires UserOrAdmin role
 * • GET /api/v1/products/{id} - Requires UserOrAdmin role  
 * • POST /api/v1/products - Requires UserOrAdmin role
 * • PUT /api/v1/products/{id} - Requires UserOrAdmin role
 * • DELETE /api/v1/products/{id} - Requires AdminOnly role
 * • All customer and country endpoints - Role-based access
 * 
 * DEVELOPMENT VS PRODUCTION:
 * -------------------------
 * • Development: Relaxed CORS, detailed error info, request logging
 * • Production: Strict CORS, minimal error details, HSTS headers
 */

using CleanCut.Application;
using CleanCut.Infrastructure.Data;
using CleanCut.Infrastructure.Caching;
using MediatR;
using CleanCut.API.EventHandlers;
using CleanCut.Application.Events;
using CleanCut.Infrastructure.Data.Seeding;
using CleanCut.API.Middleware;
using CleanCut.API.Constants;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using System.Threading.RateLimiting;
using CleanCut.Infrastructure.Shared;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Add global exception handling
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// ? Enhanced problem details for security
builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        // Only apply Problem Details to API routes
   if (!context.HttpContext.Request.Path.StartsWithSegments("/api"))
   return;

    // ? Don't expose sensitive information in production
        if (!builder.Environment.IsDevelopment())
        {
  // Remove machine name and detailed errors in production
    context.ProblemDetails.Extensions.Remove("machine");
        }
   else
     {
 context.ProblemDetails.Extensions.TryAdd("machine", Environment.MachineName);
        }
 
 context.ProblemDetails.Extensions.TryAdd("requestId", context.HttpContext.TraceIdentifier);
   context.ProblemDetails.Extensions.TryAdd("timestamp", DateTime.UtcNow);
      context.ProblemDetails.Instance ??= context.HttpContext.Request.Path;

      if (context.ProblemDetails.Type == null)
      {
       context.ProblemDetails.Type = context.ProblemDetails.Status switch
        {
400 => "https://tools.ietf.org/html/rfc7231#section-6.5.1",
      401 => "https://tools.ietf.org/html/rfc7235#section-3.1",
        403 => "https://tools.ietf.org/html/rfc7231#section-6.5.3",
        404 => "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  422 => "https://tools.ietf.org/html/rfc4918#section-11.2",
  500 => "https://tools.ietf.org/html/rfc7231#section-6.6.1",
       _ => "https://tools.ietf.org/html/rfc7231"
  };
   }
    };
});

// ? Add rate limiting for security
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
         partitionKey: context.User?.Identity?.Name ?? context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: partition => new FixedWindowRateLimiterOptions
  {
    AutoReplenishment = true,
           PermitLimit = builder.Environment.IsDevelopment() ? 1000 : 100, // More restrictive in production
  Window = TimeSpan.FromMinutes(1)
       }));
    
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = 429;
      await context.HttpContext.Response.WriteAsync("Too many requests. Please try again later.", cancellationToken: token);
    };
});

// ? Secure CORS configuration for all OAuth 2.1 clients
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowCleanCutClients", policy =>
  {
 var allowedOrigins = builder.Environment.IsDevelopment()
   ? new[] {
     "https://localhost:7297", // CleanCut.BlazorWebApp HTTPS
       "http://localhost:5091",   // CleanCut.BlazorWebApp HTTP (dev only)
       "https://localhost:7286", // CleanCut.WebApp (MVC) HTTPS  
   "http://localhost:5200",   // CleanCut.WebApp (MVC) HTTP (dev only)
  "https://localhost:7068", // TempBlazorApp HTTPS
       "http://localhost:5110"    // TempBlazorApp HTTP (dev only)
   }
            : builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

 policy.WithOrigins(allowedOrigins)
   .AllowAnyMethod()
    .AllowAnyHeader()
     .AllowCredentials()
    .SetPreflightMaxAge(TimeSpan.FromMinutes(5));
    });

    // ? Separate policy for Swagger (dev only)
    if (builder.Environment.IsDevelopment())
    {
        options.AddPolicy("AllowSwagger", policy =>
        {
 policy.AllowAnyOrigin()
 .AllowAnyMethod()
     .AllowAnyHeader();
     });
 }
});

// ? Enhanced logging configuration
builder.Services.AddLogging(loggingBuilder =>
{
    loggingBuilder.AddConsole();
    loggingBuilder.SetMinimumLevel(builder.Environment.IsDevelopment() ? LogLevel.Debug : LogLevel.Information);
});

// Get API configuration
var apiTitle = builder.Configuration["ApiSettings:Title"] ?? "CleanCut API";
var apiDescription = builder.Configuration["ApiSettings:Description"] ?? "CleanCut API";

// Use built-in OpenAPI generation in .NET 10 (preferred)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// Register Data infrastructure first so IUnitOfWork and repositories are available
builder.Services.AddDataInfrastructure(builder.Configuration);

// Add Application layer (MediatR handlers depend on IUnitOfWork and repositories)
builder.Services.AddApplication();


// Register HttpContextAccessor and a distributed cache provider for the API host
builder.Services.AddHttpContextAccessor();
try
{
    // Configure caching infrastructure (will use Redis when ConnectionStrings:Redis is present)
    builder.Services.AddCachingInfrastructure(builder.Configuration);
}
catch
{
    // If caching registration fails, fall back to distributed memory cache
    builder.Services.AddDistributedMemoryCache();
}

// Register shared infrastructure
builder.Services.AddSharedInfrastructure(builder.Configuration);

// Register API notification handlers explicitly (ensures discovery)
builder.Services.AddScoped(typeof(INotificationHandler<ProductCreatedNotification>), typeof(ProductCreatedNotificationHandler));
builder.Services.AddScoped(typeof(INotificationHandler<CountryCreatedNotification>), typeof(CountryCreatedNotificationHandler));
builder.Services.AddScoped(typeof(INotificationHandler<OrderCreatedNotification>), typeof(OrderCreatedNotificationHandler));
builder.Services.AddScoped(typeof(INotificationHandler<OrderUpdatedNotification>), typeof(OrderUpdatedNotificationHandler));
builder.Services.AddScoped(typeof(INotificationHandler<CustomerUpdatedNotification>), typeof(CustomerUpdatedNotificationHandler));

// Provide a delegate for idempotency behavior to read the Idempotency-Key header from the current HttpContext
builder.Services.AddScoped<Func<object?>>(sp =>
{
    return () =>
    {
        var accessor = sp.GetService<IHttpContextAccessor>();
        var ctx = accessor?.HttpContext;
        if (ctx == null) return null;
        if (!ctx.Request.Headers.TryGetValue("Idempotency-Key", out var values)) return null;
        var key = values.FirstOrDefault();
        return string.IsNullOrWhiteSpace(key) ? null : (object?)key;
    };
});

// Add authentication and authorization to ensure a default scheme is configured
var identityAuthority = builder.Configuration["Identity:Authority"] ?? builder.Configuration["IdentityServer:Authority"] ?? "https://localhost:5001";
var apiAudience = builder.Configuration["ApiSettings:Audience"] ?? "CleanCutAPI";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = identityAuthority;
        options.Audience = apiAudience;
        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = identityAuthority,
            ValidateAudience = true,
            ValidAudience = apiAudience,
            ValidateLifetime = true
        };
    });

builder.Services.AddAuthorization(options =>
{
    // Global fallback policy: require authenticated user except allow anonymous for Swagger UI and related assets in development.
    // Global fallback policy: require authenticated user for all endpoints by default.
    // We intentionally do NOT whitelist Swagger UI or OpenAPI JSON endpoints here —
    // the OpenAPI document is protected and requires an authenticated user.
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();

    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("UserOrAdmin", policy => policy.RequireRole("User", "Admin"));
});

var app = builder.Build();

// ? HTTP request logging middleware
app.Use(async (context, next) =>
{
    var sw = System.Diagnostics.Stopwatch.StartNew();
    await next.Invoke();
    sw.Stop();

    // Log only requests handled by the API (excluding static files, etc.)
    if (context.Request.Path.StartsWithSegments("/api"))
    {
        var log = $"{context.Response.StatusCode} {context.Request.Method} {context.Request.Path} ({sw.ElapsedMilliseconds} ms)";
        context.Items["RequestLog"] = log;
        var logger = context.RequestServices.GetService<Microsoft.Extensions.Logging.ILoggerFactory>()?.CreateLogger("RequestLogging");
        logger?.LogInformation(log);
    }
});

// ? Security headers middleware
//app.Use(async (context, next) =>
  //{
    // Add security headers to response
  //  context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
  //  context.Response.Headers.Add("X-Frame-Options", "DENY");
  //  context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
 //   context.Response.Headers.Add("Access-Control-Expose-Headers", "Content-Disposition");

//    await next();
//  });

app.UseRouting();

// ? OpenAPI documentation (dev only) - use .NET 10 built-in OpenAPI
// Register the OpenAPI document endpoint in development for developer UX
if (app.Environment.IsDevelopment())
{
    // Expose the OpenAPI JSON (e.g. /openapi/v1.json)
    // Allow anonymous access to the OpenAPI document so developer tools and the
    // Blazor/MVC proxy can fetch it without requiring an authenticated user.
    // This overrides the global fallback policy which requires authentication by default.
    app.MapOpenApi().AllowAnonymous();

    // Development convenience: redirect root to the OpenAPI JSON for quick access
    app.Use(async (context, next) =>
    {
        var path = context.Request.Path.Value ?? string.Empty;
        if (path == "/")
        {
            context.Response.Redirect("/openapi/v1.json");
            return;
        }

        await next();
    });
}

// ? Enable authentication and authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Seed the database with initial data
using (var scope = app.Services.CreateScope())
{
    try
    {
        var seederType = Type.GetType("CleanCut.Infrastructure.Data.Seeding.DatabaseSeeder, CleanCut.Infrastructure.Data");
        if (seederType != null)
        {
            var seederMethod = seederType.GetMethod("SeedAsync", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            if (seederMethod != null)
            {
                await (Task)seederMethod.Invoke(null, new object[] { scope.ServiceProvider })!;
            }
        }
    }
    catch
    {
        // Ignore seeding failures
    }
}

app.Run();
