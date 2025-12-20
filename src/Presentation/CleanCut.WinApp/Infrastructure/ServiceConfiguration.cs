using System;
using System.IO;
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
using AutoMapper;
using CleanCut.WinApp.Views.Countries;


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
        services.AddSingleton<IConfiguration>(configuration);

        // Logging
        ConfigureLogging(services);

        // Core infrastructure registration order matters:
        // - Data and shared infrastructure (repositories, etc.) must be registered
        //   before the Application layer so MediatR pipeline behaviors that depend on those
        //   services can be resolved during DI validation.
        services.AddDataInfrastructure(configuration);
        services.AddCachingInfrastructure(configuration);
        services.AddApplication(includeIntegrationBehaviors: false);


        // AutoMapper profiles for WinApp viewmodel <-> application DTO mapping
        // Configure AutoMapper via the AddAutoMapper action overload
        services.AddAutoMapper(cfg => cfg.AddProfile(new CleanCut.WinApp.Infrastructure.Mapping.WinAppMappingProfile()));


        // Customer Management MVP components
        services.AddTransient<ICustomerListView, CustomerListForm>();
        services.AddTransient<ICustomerEditView, CustomerEditForm>();
        services.AddTransient<CustomerListPresenter>();
        services.AddTransient<CustomerEditPresenter>();

   
        // Product Management MVP components
        services.AddTransient<IProductListView, ProductListForm>();
        services.AddTransient<IProductEditView, ProductEditForm>();
        services.AddTransient<ProductListPresenter>();
        services.AddTransient<ProductEditPresenter>();

        // Country Management MVP components
        services.AddTransient<ICountryListView, CountryListForm>();
        services.AddTransient<ICountryEditView, CountryEditForm>();
        services.AddTransient<CountryListPresenter>();
        services.AddTransient<CountryEditPresenter>();

        // Order Management MVP components
        services.AddTransient<CleanCut.WinApp.Views.Orders.IOrderListView, CleanCut.WinApp.Views.Orders.OrderListForm>();
        services.AddTransient<CleanCut.WinApp.Views.Orders.IOrderLineItemListView, CleanCut.WinApp.Views.Orders.OrderLineItemListForm>();
        services.AddTransient<CleanCut.WinApp.Views.Orders.IOrderLineItemEditView, CleanCut.WinApp.Views.Orders.OrderLineItemEditForm>();
        services.AddTransient<CleanCut.WinApp.Presenters.OrderListPresenter>();
        services.AddTransient<CleanCut.WinApp.Presenters.OrderLineItemListPresenter>();



        services.AddTransient(typeof(Services.Factories.IViewFactory<>), typeof(Services.Factories.ViewFactory<>));


        // Command factory (presentation layer helper to construct App commands from viewmodels)
        services.AddTransient<Services.ICommandFactory, Services.CommandFactory>();

        // Register product edit view factory
        services.AddTransient<Services.Factories.IViewFactory<IProductEditView>, Services.Factories.ViewFactory<IProductEditView>>();

        // IMapper is registered by AddAutoMapper; presenters can request IMapper directly

        // Main form factory
        services.AddTransient<MainForm>();

        // Navigation service
        services.AddTransient<Services.Navigation.INavigationService, Services.Navigation.NavigationService>();

        // Management loader (testable helper to create presenters/views in a scope)
        services.AddSingleton<Services.Management.IManagementLoader, Services.Management.ManagementLoader>();

        // Presenter factories will be resolved via ActivatorUtilities; view factories registered above


        // Enable scope validation and validate-on-build in non-production environments to catch DI mistakes early.
        var environmentName = configuration["ENVIRONMENT"] ?? Environment.GetEnvironmentVariable("ENVIRONMENT") ?? "Development";
        var validateOnBuild = !environmentName.Equals("Production", StringComparison.OrdinalIgnoreCase);

        return services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateScopes = true,
            ValidateOnBuild = validateOnBuild
        });
    }

    private static IConfiguration BuildConfiguration()
    {
        var environment = Environment.GetEnvironmentVariable("ENVIRONMENT") ?? "Development";

        return new ConfigurationBuilder()
            .SetBasePath(GetProjectBasePath())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();
    }

    private static void ConfigureLogging(IServiceCollection services)
    {
        var basePath = GetProjectBasePath();

        // Ensure logs directory exists (use absolute path under the project folder)
        var logDirectory = Path.Combine(basePath, "logs");
        if (!Directory.Exists(logDirectory))
        {
            Directory.CreateDirectory(logDirectory);
        }

        // Use an absolute path for the Serilog file sink so it's deterministic
        var logFilePath = Path.Combine(logDirectory, "cleancut-winapp-.txt");

        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File(path: logFilePath, rollingInterval: RollingInterval.Day)
            .CreateLogger();

        // Emit startup information to help locate the files
        Log.Information("Serilog configured. Logs folder: {LogDirectory}", logDirectory);
        Console.WriteLine($"Serilog configured. Logs folder: {logDirectory}");

        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddSerilog();
        });

        // Register non-generic ILogger to help components that request ILogger (non-generic)
        // Some presenters or factories may mistakenly request non-generic ILogger; provide
        // a default logger instance created from the ILoggerFactory to avoid DI failures.
        services.AddSingleton<Microsoft.Extensions.Logging.ILogger>(provider =>
        {
            var factory = provider.GetRequiredService<Microsoft.Extensions.Logging.ILoggerFactory>();
            return factory.CreateLogger("CleanCut.WinApp");
        });
    }

    private static string GetProjectBasePath()
    {
        var startDir = new DirectoryInfo(AppContext.BaseDirectory ?? Directory.GetCurrentDirectory());
        DirectoryInfo? projectDir = startDir;
        while (projectDir is not null &&
               !File.Exists(Path.Combine(projectDir.FullName, "CleanCut.WinApp.csproj")))
        {
            projectDir = projectDir.Parent;
        }

        return projectDir?.FullName ?? AppContext.BaseDirectory ?? Directory.GetCurrentDirectory();
    }
}
