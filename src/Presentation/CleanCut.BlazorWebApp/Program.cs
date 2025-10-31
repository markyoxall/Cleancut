using CleanCut.BlazorWebApp.Components;
using CleanCut.BlazorWebApp.Services;
using CleanCut.BlazorWebApp.State;
using CleanCut.BlazorWebApp.Extensions;

namespace CleanCut.BlazorWebApp;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // ? Add data protection for secure token storage
        builder.Services.AddDataProtection(options =>
        {
            options.ApplicationDiscriminator = "CleanCut.BlazorWebApp";
        });

        // Add services to the container.
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();

        // Configure Blazor Server options for better error handling and security
        builder.Services.Configure<Microsoft.AspNetCore.Components.Server.CircuitOptions>(options =>
        {
            if (builder.Environment.IsDevelopment())
            {
                options.DetailedErrors = true;
            }

            // ? Security settings
            options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(3);
            options.DisconnectedCircuitMaxRetained = 100;
            options.JSInteropDefaultCallTimeout = TimeSpan.FromMinutes(1);
        });

        builder.Services.AddHttpContextAccessor();

        // ? Register ALL API clients with enhanced security
        builder.Services.AddProductApiClients(builder.Configuration);

        // Other services
        builder.Services.AddScoped<IUiStateService, UiStateService>();

        // Register feature state services
        builder.Services.AddScoped<ICustomersState, CustomeraState>();
        builder.Services.AddScoped<IProductsState, ProductsState>();
        builder.Services.AddScoped<ICountriesState, CountriesState>();

        // ? Add HSTS for production
        if (!builder.Environment.IsDevelopment())
        {
            builder.Services.AddHsts(options =>
            {
                options.Preload = true;
                options.IncludeSubDomains = true;
                options.MaxAge = TimeSpan.FromDays(365);
            });
        }

        // ? Enhanced logging
        builder.Services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.AddConsole();
            if (builder.Environment.IsDevelopment())
            {
                loggingBuilder.SetMinimumLevel(LogLevel.Debug);
            }
            else
            {
                loggingBuilder.SetMinimumLevel(LogLevel.Information);
            }
        });

        var app = builder.Build();

        // ? Enhanced security middleware pipeline
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        // ? Add security headers middleware
        app.Use(async (context, next) =>
        {
            var headers = context.Response.Headers;

            headers.TryAdd("X-Content-Type-Options", "nosniff");
            headers.TryAdd("X-Frame-Options", "DENY");
            headers.TryAdd("X-XSS-Protection", "1; mode=block");
            headers.TryAdd("Referrer-Policy", "strict-origin-when-cross-origin");

            if (!app.Environment.IsDevelopment())
            {
                headers.TryAdd("Strict-Transport-Security", "max-age=31536000; includeSubDomains; preload");
            }

            // ? Enhanced CSP for Blazor Server
            var csp = "default-src 'self'; " +
                "script-src 'self' 'unsafe-inline' 'unsafe-eval'; " + // Blazor Server needs inline scripts
                "style-src 'self' 'unsafe-inline'; " +
                "img-src 'self' data: https:; " +
                "connect-src 'self' wss: https://localhost:5001 https://localhost:7142; " + // WebSocket for SignalR
                "font-src 'self'; " +
                "object-src 'none'; " +
                "base-uri 'self'; " +
                "form-action 'self'; " +
                "frame-ancestors 'none'";

            if (!app.Environment.IsDevelopment())
            {
                csp += "; upgrade-insecure-requests";
            }

            headers.TryAdd("Content-Security-Policy", csp);

            await next();
        });

        app.UseStaticFiles();
        app.UseRouting();
        app.UseAntiforgery();

        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        app.Run();
    }
}
