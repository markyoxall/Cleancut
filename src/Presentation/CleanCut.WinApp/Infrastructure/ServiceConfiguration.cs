using CleanCut.Application;
using CleanCut.Infrastructure.Caching;
using CleanCut.Infrastructure.Data;
using CleanCut.WinApp.Infrastructure.Mapping;
using CleanCut.WinApp.Presenters;
using CleanCut.WinApp.Services;
using CleanCut.WinApp.Services.Caching;
using CleanCut.WinApp.Services.Factories;
using CleanCut.WinApp.Services.Management;
using CleanCut.WinApp.Views.Countries;
using CleanCut.WinApp.Views.Customers;
using CleanCut.WinApp.Views.Orders;
using CleanCut.WinApp.Views.Products;
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
        services.AddSingleton<IConfiguration>(configuration);

        // Logging
        ConfigureLogging(services);

        // Core infrastructure registration order matters:

        // - Data and shared infrastructure (repositories, etc.) must be registered
        //   before the Application layer so MediatR pipeline behaviors that depend on those
        //   services can be resolved during DI validation.
        // Data layer
        // Register data infrastructure (DbContext and repositories).
        // NOTE: `AddDbContext<T>` registers the EF Core `DbContext` as scoped by default.
        // Scoped lifetime means a new `CleanCutDbContext` instance is created per DI scope
        // (for example per web request or per `IServiceScope` created manually).
        // Do NOT register DbContext as a singleton — resolving it from a singleton will
        // capture the same DbContext instance across scopes which can cause threading
        // and stale-data issues.
        services.AddDataInfrastructure(configuration);
        // Caching infrastructure depends on data infrastructure
        services.AddCachingInfrastructure(configuration);
        // Application layer uses MediatR and pipeline behaviors that depend on infrastructure services
        services.AddApplication(includeIntegrationBehaviors: false);


        // AutoMapper profiles for WinApp viewmodel <-> application DTO mapping
        // Configure AutoMapper via the AddAutoMapper action overload
        // AutoMapper registrations are typically singleton in nature (profiles are
        // registered and the mapper instance is thread-safe). Consumers can request
        // `IMapper` as a singleton or transient without causing issues.
        services.AddAutoMapper(cfg => cfg.AddProfile(new WinAppMappingProfile()));


        // Customer Management MVP components
        // Views and presenters use the transient lifetime so a new instance is created
        // each time a view or presenter is requested. This avoids sharing UI state
        // across usages and works well when presenters are created inside an
        // `IServiceScope` (see `ManagementLoader`) which provides scoped services
        // like `CleanCutDbContext` to those presenters when required.
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
        services.AddTransient<IOrderListView, OrderListForm>();
        services.AddTransient<IOrderLineItemListView, OrderLineItemListForm>();
        services.AddTransient<IOrderLineItemEditView, OrderLineItemEditForm>();
        services.AddTransient<OrderListPresenter>();
        services.AddTransient<OrderLineItemListPresenter>();

        // IViewFactory registrations for views used by presenters to create views dynamically at runtime 
        // Register the concrete generic factory type so the DI container can resolve IViewFactory<T>
        services.AddTransient(typeof(IViewFactory<>), typeof(ViewFactory<>));


        // Command factory (presentation layer helper to construct App commands from viewmodels)
        services.AddTransient<ICommandFactory, CommandFactory>();

        // Register product edit view factory
        services.AddTransient<IViewFactory<IProductEditView>, ViewFactory<IProductEditView>>();

        // IMapper is registered by AddAutoMapper; presenters can request IMapper directly

        // Main form factory
        // The main form is transient so callers get a fresh instance each time.
        services.AddTransient<MainForm>();


        // Management loader (testable helper to create presenters/views in a scope)
        // `ManagementLoader` is registered as a singleton because it is a lightweight
        // factory that creates `IServiceScope`s for each management screen. It must
        // NOT capture scoped services in its constructor. Any scoped dependencies
        // (for example a DB-backed `IUserPreferencesService`) must be resolved from
        // the scope created inside the loader methods so that `CleanCutDbContext`
        // and other scoped services are created per scope and disposed correctly.
        services.AddSingleton<IManagementLoader, ManagementLoader>();

        // Register a minimal global exception handler for the WinApp host so the
        // Program startup code can resolve and use it to route unhandled exceptions.
        // Use transient so logger dependencies are resolved per request; the handler
        // is stateless.
        services.AddTransient<CleanCut.Application.Handlers.GlobalException.IGlobalExceptionHandler, CleanCut.Application.Handlers.GlobalException.WinAppGlobalExceptionHandler>();


        // Register preferences service choices — use configuration to decide
        var prefStore = configuration.GetValue<string>("Preferences:Store") ?? "file";

        if (prefStore.Equals("db", StringComparison.OrdinalIgnoreCase))
        {
            // Database-backed preferences service depends on `CleanCutDbContext` and
            // thus must be registered as scoped so it is created per DI scope along
            // with the DbContext.
            services.AddScoped<IUserPreferencesService, DatabaseUserPreferencesService>();
        }
        else
        {
            // File-based preferences service does not depend on scoped services and
            // can be safely registered as a singleton for the lifetime of the app.
            services.AddSingleton<IUserPreferencesService, UserPreferencesService>();
        }

        // Layout persistence service used by presenters to save/restore grid layouts.
        // File-based implementation is stateless so register as singleton. If a
        // database-backed implementation is required, switch to scoped and implement
        // accordingly.
        services.AddSingleton<ILayoutPersistenceService, FileLayoutPersistenceService>();
        // Register cache manager used by presenters to centralize invalidation logic
        // Cache manager and its logging wrapper are transient so callers receive a
        // fresh instance; they may internally resolve scoped services from the
        // current scope as needed.
        services.AddTransient<CacheManager>();
        services.AddTransient<ICacheManager>(sp => new LoggingCacheManager(sp.GetRequiredService<CacheManager>(), sp.GetRequiredService<ILogger<LoggingCacheManager>>()));


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
        Serilog.Log.Logger = new Serilog.LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File(path: logFilePath, rollingInterval: Serilog.RollingInterval.Day)
            .CreateLogger();

        // Emit startup information to help locate the files
        Serilog.Log.Information("Serilog configured. Logs folder: {LogDirectory}", logDirectory);
        Console.WriteLine($"Serilog configured. Logs folder: {logDirectory}");

        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddSerilog();
        });

        // Register non-generic ILogger to help components that request ILogger (non-generic)
        // Some presenters or factories may mistakenly request non-generic ILogger; provide
        // a default logger instance created from the ILoggerFactory to avoid DI failures.
        // This singleton logger is safe because ILogger instances are lightweight wrappers
        // produced by the logger factory and are thread-safe.
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
               !File.Exists(Path.Combine(projectDir.FullName, "csproj")))
        {
            projectDir = projectDir.Parent;
        }

        return projectDir?.FullName ?? AppContext.BaseDirectory ?? Directory.GetCurrentDirectory();
    }
}
