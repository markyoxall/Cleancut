using CleanCut.Application;
using CleanCut.Infrastructure.Data;
using CleanCut.Infrastructure.Caching;
using CleanCut.Infrastructure.Data.Seeding;
using CleanCut.API.Middleware;
using CleanCut.API.Constants;
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Add global exception handling
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// Configure Problem Details (only for API routes)
builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        // Only apply Problem Details to API routes
        if (!context.HttpContext.Request.Path.StartsWithSegments("/api"))
            return;

        // Add common extensions to all problem details
        context.ProblemDetails.Extensions.TryAdd("machine", Environment.MachineName);
        context.ProblemDetails.Extensions.TryAdd("requestId", context.HttpContext.TraceIdentifier);
        context.ProblemDetails.Extensions.TryAdd("timestamp", DateTime.UtcNow);
        
        // Add instance if not set
        context.ProblemDetails.Instance ??= context.HttpContext.Request.Path;
        
        // Add helpful links
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

// Add CORS services for both Swagger and Blazor app
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorApp", policy =>
    {
        policy.WithOrigins(
                "https://localhost:7297", // Your Blazor HTTPS port
                "http://localhost:5091",  // Your Blazor HTTP port
                "https://localhost:5001", // Common Blazor ports
                "http://localhost:5000"   // Common Blazor ports
              )
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials()
              .SetPreflightMaxAge(TimeSpan.FromMinutes(5)); // Cache preflight for 5 minutes
    });
    
    options.AddPolicy("AllowSwagger", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add logging to see CORS in action
builder.Services.AddLogging(loggingBuilder =>
{
    loggingBuilder.AddConsole();
    loggingBuilder.SetMinimumLevel(LogLevel.Information);
});

// Get API configuration
var apiTitle = builder.Configuration["ApiSettings:Title"] ?? "CleanCut API";
var apiDescription = builder.Configuration["ApiSettings:Description"] ?? "CleanCut API";

// Add OpenAPI services (.NET 10 built-in)
builder.Services.AddOpenApi();

// Add Application layer
builder.Services.AddApplication();

// Add Data Infrastructure layer
builder.Services.AddDataInfrastructure(builder.Configuration);

// Add Caching Infrastructure layer
builder.Services.AddCachingInfrastructure(builder.Configuration);

// Add JWT Bearer authentication for IdentityServer
builder.Services.AddAuthentication(options =>
{
 options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
 options.Authority = builder.Configuration["IdentityServer:Authority"] ?? "https://localhost:5003";
 options.Audience = "CleanCutAPI";
 options.RequireHttpsMetadata = true;
});

// Add authorization and require authentication globally
builder.Services.AddAuthorization(options =>
{
 options.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
 .RequireAuthenticatedUser()
 .Build();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // Add request logging middleware first
    app.Use(async (context, next) =>
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Incoming request: {Method} {Path} from {Origin}", 
            context.Request.Method, 
            context.Request.Path, 
            context.Request.Headers.Origin.FirstOrDefault() ?? "No Origin");
        
        await next();
        
        logger.LogInformation("Response: {StatusCode} for {Method} {Path}", 
            context.Response.StatusCode, 
            context.Request.Method, 
            context.Request.Path);
    });
    
    // Enable CORS for both Blazor app and Swagger
    app.UseCors("AllowBlazorApp");
    
    // Serve static files first (before routing)
    app.UseStaticFiles();
    
    // Add custom routes for landing pages
    app.MapGet("/", () => Results.Redirect("/versions.html"));
    app.MapGet("/index.html", () => Results.Redirect("/versions.html"));
    app.MapGet("/versions", () => Results.Redirect("/versions.html"));
    
    // Map OpenAPI endpoint
    app.MapOpenApi();
    
    // Add Swagger UI (shows all endpoints from both versions)
    app.UseSwaggerUI(options =>
    {
        // Use canonical endpoints/titles from ApiConstants
        options.SwaggerEndpoint(ApiConstants.OpenApiJson, ApiConstants.SwaggerUiTitle);
        options.RoutePrefix = ApiConstants.SwaggerUiRoutePrefix;
        options.DocumentTitle = ApiConstants.SwaggerUiTitle;
        options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
        options.DisplayRequestDuration();
        options.EnableDeepLinking();
        options.ShowExtensions();
        
        // Add custom CSS to highlight different versions
        options.InjectStylesheet("/custom-swagger.css");
    });
    
    // Seed the database in development
    using (var scope = app.Services.CreateScope())
    {
        await DatabaseSeeder.SeedAsync(scope.ServiceProvider);
    }
}

// Use global exception handler
app.UseExceptionHandler();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
