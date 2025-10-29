using CleanCut.WebApp.Services.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CleanCut.WebApp.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiClients(this IServiceCollection services, IConfiguration configuration)
    {
        // Read API client configuration
   var apiConfig = configuration.GetSection("ApiClients").Get<ApiClientOptions>() ?? new ApiClientOptions();
        
     // Register the token service for client credentials authentication
        services.AddHttpClient<ITokenService, TokenService>();
        services.AddScoped<ITokenService, TokenService>();

   // Register the authenticated message handler
     services.AddScoped<AuthenticatedHttpMessageHandler>();

   // ? Configure authenticated HttpClient for Customer API
        services.AddHttpClient<ICustomerApiService, CustomerApiService>(client =>
        {
 client.BaseAddress = new Uri(apiConfig.BaseUrl);
          client.Timeout = TimeSpan.FromSeconds(apiConfig.TimeoutSeconds);
        })
        .AddHttpMessageHandler<AuthenticatedHttpMessageHandler>();

    // ? Configure authenticated HttpClient for Product API  
   services.AddHttpClient<IProductApiService, ProductApiService>(client =>
        {
    client.BaseAddress = new Uri(apiConfig.BaseUrl);
     client.Timeout = TimeSpan.FromSeconds(apiConfig.TimeoutSeconds);
        })
        .AddHttpMessageHandler<AuthenticatedHttpMessageHandler>();

        return services;
    }
}

public class ApiClientOptions
{
    public string BaseUrl { get; set; } = "https://localhost:7142";
    public int TimeoutSeconds { get; set; } = 30;
}