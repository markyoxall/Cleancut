using CleanCut.Application;
using CleanCut.Infrastructure.Caching;
using CleanCut.Infrastructure.Data;
using CleanCut.WinApp.Presenters;
using CleanCut.WinApp.Views.Users;
using CleanCut.WinApp.Views.Products;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

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
        
        // Core layers
        services.AddApplication();
        services.AddDataInfrastructure(configuration);
        services.AddCachingInfrastructure(configuration);
        
        // User Management MVP components
        services.AddScoped<IUserListView, UserListForm>();
        services.AddScoped<IUserEditView, UserEditForm>();
        services.AddScoped<UserListPresenter>();
        services.AddScoped<UserEditPresenter>();
        
        // Product Management MVP components
        services.AddScoped<IProductListView, ProductListForm>();
        services.AddScoped<IProductEditView, ProductEditForm>();
        services.AddScoped<ProductListPresenter>();
        services.AddScoped<ProductEditPresenter>();
        
        // Main form factory
        services.AddScoped<MainForm>();
        
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