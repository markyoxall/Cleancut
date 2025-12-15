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

        //
      
        services.AddTransient<AuthenticatedHttpMessageHandler>();


        // ? Configure authenticated HttpClient for Customer API with USER tokens
        services.AddHttpClient<ICustomerApiService, CustomerApiService>(client =>
             {
                 var baseUrl = (apiConfig.BaseUrl ?? "").TrimEnd('/') + "/";
                 client.BaseAddress = new Uri(baseUrl);
                 client.Timeout = TimeSpan.FromSeconds(apiConfig.TimeoutSeconds);
             })
             .AddHttpMessageHandler<AuthenticatedHttpMessageHandler>();

        // ? Configure authenticated HttpClient for Product API with USER tokens
        services.AddHttpClient<IProductApiService, ProductApiService>(client =>
             {
                 var baseUrl = (apiConfig.BaseUrl ?? "").TrimEnd('/') + "/";
                 client.BaseAddress = new Uri(baseUrl);
                 client.Timeout = TimeSpan.FromSeconds(apiConfig.TimeoutSeconds);
             })
             .AddHttpMessageHandler<AuthenticatedHttpMessageHandler>();

        // ? Configure authenticated HttpClient for Country API with USER tokens
        services.AddHttpClient<ICountryApiService, CountryApiService>(client =>
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
