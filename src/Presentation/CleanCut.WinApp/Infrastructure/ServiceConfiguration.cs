/*
 * WinForms Service Configuration and Dependency Injection
 * =======================================================
 * 
 * This file configures dependency injection and service registration for the CleanCut
 * WinForms application. It establishes the foundation for authentication and API
 * integration in the desktop client application.
 * 
 * ROLE IN AUTHENTICATION ARCHITECTURE:
 * ------------------------------------
 * This configuration sets up the service layer that will support:
 * 
 * 1. AUTHENTICATION SERVICES - OAuth2/OIDC client for IdentityServer integration
 * 2. API CLIENTS - HTTP clients for authenticated CleanCut.API communication
 * 3. TOKEN MANAGEMENT - Secure storage and refresh of authentication tokens
 * 4. MVP INTEGRATION - Dependency injection for presenters and services
 * 
 * PLANNED AUTHENTICATION SERVICE REGISTRATION:
 * -------------------------------------------
 * 
 * • IAuthenticationService (Future):
 *   ??? OAuth2 Authorization Code + PKCE flow implementation
 *   ??? System browser integration for IdentityServer login
 *   ??? Local token storage using Windows Data Protection
 *   ??? Automatic token refresh and lifecycle management
 * 
 * • ITokenStorageService (Future):
 *   ??? Secure token persistence using Windows DPAPI
 *   ??? User-specific encryption keys
 *   ??? Automatic cleanup on logout/uninstall
 *   ??? Protection against local privilege escalation
 * 
 * • HttpClient Configuration (Future):
 *   ??? Authenticated HTTP clients for API communication
 *   ??? Automatic Bearer token injection
 *   ??? Base URL configuration for CleanCut.API
 *   ??? Timeout and retry policies
 * 
 * DESKTOP-SPECIFIC SERVICE PATTERNS:
 * ----------------------------------
 * 
 * • Singleton Services:
 *   ??? Authentication state management across forms
 *   ??? Token cache shared between API calls
 *   ??? Configuration services for app settings
 * 
 * • Scoped Services:
 *   ??? Form-specific presenters and view models
 *   ??? API services tied to user sessions
 *   ??? Transaction-scoped database operations
 * 
 * • Transient Services:
 *   ??? HTTP clients for individual API requests
 *   ??? Utility services without state
 *   ??? Factory services for dynamic object creation
 * 
 * INTEGRATION WITH MVP PATTERN:
 * -----------------------------
 * The service configuration supports the MVP (Model-View-Presenter) architecture:
 * 
 * • Presenters (Already Implemented):
 *   ??? ProductListPresenter, ProductEditPresenter
 *   ??? CustomerListPresenter, CustomerEditPresenter
 *   ??? Will receive authenticated API services via dependency injection
 * 
 * • API Services (Planned):
 *   ??? IProductApiService for product CRUD operations
 *   ??? ICustomerApiService for customer management
 *   ??? All will use authenticated HTTP clients automatically
 * 
 * • Views (Forms):
 *   ??? Existing forms will receive presenters with authenticated services
 *   ??? Authentication state reflected in UI elements
 *   ??? Login/logout functionality in main form
 * 
 * AUTHENTICATION FLOW INTEGRATION:
 * --------------------------------
 * 
 * 1. Application Startup:
 *    ??? ServiceConfiguration.ConfigureServices() sets up DI container
 *    ??? Authentication services registered as singletons
 *    ??? Check for existing valid tokens in secure storage
 * 
 * 2. User Authentication:
 *    ??? AuthenticationService launches system browser
 *    ??? User authenticates against IdentityServer
 *    ??? Tokens stored securely and injected into API services
 * 
 * 3. API Communication:
 *    ??? Presenters use authenticated API services
 *    ??? HTTP clients automatically include Bearer tokens
 * ??? Token refresh handled transparently
 * 
 * 4. Application Shutdown:
 *    ??? Secure cleanup of cached tokens
 *    ??? Graceful service disposal
 * 
 * SECURITY CONFIGURATION:
 * ----------------------
 * 
 * • HTTP Client Security:
 *   ??? HTTPS-only communication enforced
 *   ??? Certificate validation in production
 *   ??? Request timeout policies
 *   ??? Retry policies with exponential backoff
 * 
 * • Token Storage Security:
 *   ??? Windows Data Protection API integration
 *   ??? User-specific encryption keys
 *   ??? Secure memory handling for tokens
 *   ??? Automatic token cleanup policies
 * 
 * CONFIGURATION MANAGEMENT:
 * -------------------------
 * 
 * • Application Settings:
 *   ??? IdentityServer authority URL
 *   ??? CleanCut.API base URL
 *   ??? Client configuration (ID, redirect URIs)
 *   ??? Token lifetime and refresh policies
 * 
 * • Environment-Specific Settings:
 *   ??? Development: localhost URLs, relaxed security
 *   ??? Production: production URLs, strict security
 *   ??? Configuration file or registry-based settings
 * 
 * CURRENT VS. PLANNED IMPLEMENTATION:
 * ----------------------------------
 * 
 * Current Status:
 * • ? Basic DI container setup
 * • ? MVP pattern service registration
 * • ? Form and presenter lifecycle management
 * 
 * Planned Enhancements:
 * • ? OAuth2/OIDC authentication services
 * • ? Secure token storage services
 * • ? Authenticated HTTP client configuration
 * • ? API service abstractions
 * • ? User session management
 * 
 * DEVELOPMENT ROADMAP:
 * -------------------
 * 1. Add OAuth2 client library (IdentityModel.OidcClient)
 * 2. Implement IAuthenticationService with PKCE support
 * 3. Create ITokenStorageService with Windows DPAPI
 * 4. Configure authenticated HttpClient services
 * 5. Update presenters to use authenticated API services
 * 6. Add authentication UI to MainForm
 * 7. Test complete authentication and API integration flow
 */

using CleanCut.Application;
using CleanCut.Infrastructure.Caching;
using CleanCut.Infrastructure.Data;
// shared infrastructure not required by WinApp (no RabbitMQ publishing)
using CleanCut.WinApp.Presenters;
using CleanCut.WinApp.Views.Customers;
using CleanCut.WinApp.Views.Products;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using MediatR;
using CleanCut.WinApp.Services;
using CleanCut.WinApp.Services;

namespace CleanCut.WinApp.Infrastructure;

/// <summary>
/// Service configuration for the WinForms application
/// </summary>
public static class ServiceConfiguration
{
    public static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();
        
        // Configuration
        var configuration = BuildConfiguration();
        services.AddSingleton(configuration);
        
        // Logging
        ConfigureLogging(services);
        
        // Core infrastructure registration order matters:
        // - Data and shared infrastructure (repositories, RabbitMQ publisher, etc.) must be registered
        //   before the Application layer so MediatR pipeline behaviors that depend on those
        //   services can be resolved during DI validation.
        services.AddDataInfrastructure(configuration);
        services.AddCachingInfrastructure(configuration);

        // WinApp does not publish to RabbitMQ nor require idempotency behavior.
        // Register Application layer without integration behaviors so handlers that depend
        // on IUnitOfWork are available but RabbitMQ/Idempotency behaviors are not wired.
        services.AddApplication(includeIntegrationBehaviors: false);
        
        // Customer Management MVP components
        services.AddScoped<ICustomerListView, CustomerListForm>();
        services.AddScoped<ICustomerEditView, CustomerEditForm>();
        services.AddScoped<CustomerListPresenter>();
        services.AddScoped<CustomerEditPresenter>();
        
        // Product Management MVP components
        services.AddScoped<IProductListView, ProductListForm>();
        services.AddScoped<IProductEditView, ProductEditForm>();
        services.AddScoped<ProductListPresenter>();
        services.AddScoped<ProductEditPresenter>();
        
        // Main form factory
        services.AddScoped<MainForm>();

        // SignalR notifications client for WinForms (singleton)
        var hubUrl = configuration["ApiSettings:NotificationsHubUrl"] ?? "https://localhost:5001/hubs/notifications";
        services.AddSingleton<INotificationsClient>(sp => new SignalRNotificationsClient(hubUrl, sp.GetRequiredService<ILogger<SignalRNotificationsClient>>()));
        // Local mediator to forward notifications to presenters
        services.AddSingleton<INotificationMediator, NotificationMediator>();

        // Note: wiring of SignalR client events to the mediator and starting the client
        // is performed at application startup (Program.Main) to avoid network calls during DI build.
        
        return services.BuildServiceProvider();
    }
    
    private static IConfiguration BuildConfiguration()
    {
        var environment = Environment.GetEnvironmentVariable("ENVIRONMENT") ?? "Development";
        
        return new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();
    }
    
    private static void ConfigureLogging(IServiceCollection services)
    {
        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File("logs/cleancut-winapp-.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();
        
        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddSerilog();
        });
    }
}
