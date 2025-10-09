using CleanCut.Application;
using CleanCut.Infrastructure.Data;
using CleanCut.Infrastructure.Data.Seeding;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

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
        policy.WithOrigins("https://localhost:5001", "http://localhost:5000") // Common Blazor ports
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
    
    options.AddPolicy("AllowSwagger", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
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
        options.SwaggerEndpoint("/openapi/v1.json", $"{apiTitle} - All Versions");
        options.RoutePrefix = "swagger";
        options.DocumentTitle = $"{apiTitle} - API Documentation (V1 & V2)";
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

// Use Problem Details middleware only for API routes
app.UseWhen(context => context.Request.Path.StartsWithSegments("/api"), 
    subApp =>
    {
        subApp.UseExceptionHandler();
        subApp.UseStatusCodePages();
    });

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
