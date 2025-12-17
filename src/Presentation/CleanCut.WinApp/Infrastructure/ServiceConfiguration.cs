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
        services.AddTransient<ICustomerListView, CustomerListForm>();
        services.AddTransient<ICustomerEditView, CustomerEditForm>();
        services.AddTransient<CustomerListPresenter>();
        services.AddTransient<CustomerEditPresenter>();

        // View factories
        services.AddTransient(typeof(Services.Factories.IViewFactory<>), typeof(Services.Factories.ViewFactory<>));

        // Product Management MVP components
        services.AddTransient<IProductListView, ProductListForm>();
        services.AddTransient<IProductEditView, ProductEditForm>();
        services.AddTransient<ProductListPresenter>();
        services.AddTransient<ProductEditPresenter>();

        // Register product edit view factory
        services.AddTransient<Services.Factories.IViewFactory<IProductEditView>, Services.Factories.ViewFactory<IProductEditView>>();

        // Main form factory
        services.AddTransient<MainForm>();

        // Navigation service
        services.AddTransient<Services.Navigation.INavigationService, Services.Navigation.NavigationService>();

        // Presenter factories will be resolved via ActivatorUtilities; view factories registered above

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
        // Determine the project directory (look for the .csproj file) so logs are created under
        // the project folder (e.g. src/Presentation/CleanCut.WinApp) rather than the runtime
        // working directory (bin/...)
        var startDir = new DirectoryInfo(AppContext.BaseDirectory ?? Directory.GetCurrentDirectory());
        DirectoryInfo? projectDir = startDir;
        while (projectDir is not null &&
               !File.Exists(Path.Combine(projectDir.FullName, "CleanCut.WinApp.csproj")))
        {
            projectDir = projectDir.Parent;
        }

        var basePath = projectDir?.FullName ?? AppContext.BaseDirectory ?? Directory.GetCurrentDirectory();

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
    }
}
