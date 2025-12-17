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

        // SignalR and notification services removed
        
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
