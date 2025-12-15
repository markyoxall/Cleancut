using CleanCut.Infrastructure.BackgroundServices.Authentication;
using CleanCut.Infrastructure.BackgroundServices.ExternalApi;
using CleanCut.Infrastructure.BackgroundServices.FileExport;
using CleanCut.Infrastructure.BackgroundServices.ProductExport;
using CleanCut.Infrastructure.BackgroundServices.OrderExport;
using CleanCut.Infrastructure.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;

namespace CleanCut.Infrastructure.BackgroundServices.Extensions;

/// <summary>
/// Extension methods for registering background services with enhanced resilience
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all background services and their dependencies with enhanced error handling
    /// </summary>
    public static IServiceCollection AddBackgroundServices(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // Configure settings from configuration sections
        services.Configure<AuthenticationSettings>(
            configuration.GetSection("Authentication"));
        services.Configure<ApiSettings>(
    configuration.GetSection("Api"));
        services.Configure<CsvSettings>(
            configuration.GetSection("Csv"));
        services.Configure<ProductExportConfiguration>(
            configuration.GetSection("ProductExport"));

        // Configure HTTP clients with authentication service and resilience policies
        services.AddHttpClient<IAuthenticationService, AuthenticationService>(client =>
        {
 client.Timeout = TimeSpan.FromSeconds(30);
  })
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler()
        {
  ServerCertificateCustomValidationCallback = (message, certificate, chain, sslPolicyErrors) =>
            {
        // Only allow this in development for self-signed certificates
           return Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";
  }
        })
.AddPolicyHandler(GetRetryPolicy())
  .AddPolicyHandler(GetCircuitBreakerPolicy());

        // Configure HTTP clients for API calls with enhanced resilience
        services.AddHttpClient<IApiService, ApiService>(client =>
   {
      client.Timeout = TimeSpan.FromSeconds(30);
        })
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler()
        {
 ServerCertificateCustomValidationCallback = (message, certificate, chain, sslPolicyErrors) =>
     {
   // Only allow this in development for self-signed certificates
    return Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";
    }
        })
    .AddPolicyHandler(GetRetryPolicy())
        .AddPolicyHandler(GetCircuitBreakerPolicy());

        // Register services
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IApiService, ApiService>();
        services.AddScoped<ICsvExportService, CsvExportService>();
        services.AddScoped<IProductExportService, ProductExportService>();

        // Order export services
        services.AddScoped<CleanCut.Infrastructure.BackgroundServices.OrderExport.IOrderExportService, CleanCut.Infrastructure.BackgroundServices.OrderExport.OrderExportService>();
        services.Configure<CleanCut.Infrastructure.BackgroundServices.OrderExport.OrderExportConfiguration>(configuration.GetSection("OrderExport"));
        services.AddHostedService<CleanCut.Infrastructure.BackgroundServices.OrderExport.OrderExportWorker>();

        // Register the background worker
        services.AddHostedService<ProductExportWorker>();

  return services;
    }

    /// <summary>
    /// Gets a retry policy for HTTP requests with exponential backoff
    /// </summary>
    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
      .HandleTransientHttpError()
            .OrResult(msg => !msg.IsSuccessStatusCode)
  .WaitAndRetryAsync(
         retryCount: 3,
      sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
          onRetry: (outcome, timespan, retryCount, context) =>
 {
         // Simple logging without complex extensions
      Console.WriteLine($"HTTP retry {retryCount} after {timespan.TotalMilliseconds}ms");
   });
 }

    /// <summary>
    /// Gets a circuit breaker policy for HTTP requests
    /// </summary>
    private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
    {
        return HttpPolicyExtensions
  .HandleTransientHttpError()
 .CircuitBreakerAsync(
     handledEventsAllowedBeforeBreaking: 3,
        durationOfBreak: TimeSpan.FromSeconds(30),
       onBreak: (result, duration) =>
   {
  Console.WriteLine($"Circuit breaker opened for {duration.TotalMilliseconds}ms");
       },
          onReset: () =>
  {
     Console.WriteLine("Circuit breaker reset");
          });
    }
}
