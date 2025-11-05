using System;
using CleanCut.BlazorWebApp.Services;
using CleanCut.BlazorWebApp.Services.HttpClients;
using CleanCut.BlazorWebApp.Services.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CleanCut.BlazorWebApp.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddProductApiClients(this IServiceCollection services, IConfiguration configuration)
    {
     // Read per-version config (fallbacks provided)
        var v1 = configuration.GetSection("ApiClients:Products:V1").Get<ProductApiOptions>() ?? new ProductApiOptions();
     var v2 = configuration.GetSection("ApiClients:Products:V2").Get<ProductApiOptions>() ?? new ProductApiOptions();
 
        // Read customer API config with consistent pattern
        var customerConfig = configuration.GetSection("ApiClients:Customer").Get<CustomerApiOptions>() ?? new CustomerApiOptions();
        
        // Read country API config with consistent pattern  
    var countryConfig = configuration.GetSection("ApiClients:Country").Get<CountryApiOptions>() ?? new CountryApiOptions();
     
        // Fallback to general ApiClients:BaseUrl if Customer section doesn't exist
        if (customerConfig.BaseUrl == "https://localhost:7142") // default value, try config
     {
   var fallbackBaseUrl = configuration["ApiClients:BaseUrl"];
    if (!string.IsNullOrEmpty(fallbackBaseUrl))
       {
    customerConfig.BaseUrl = fallbackBaseUrl;
    }
      }
    
// Fallback to general ApiClients:BaseUrl if Country section doesn't exist
        if (countryConfig.BaseUrl == "https://localhost:7142") // default value, try config
 {
   var fallbackBaseUrl = configuration["ApiClients:BaseUrl"];
   if (!string.IsNullOrEmpty(fallbackBaseUrl))
            {
 countryConfig.BaseUrl = fallbackBaseUrl;
       }
        }

     // ✅ Register ONLY the authenticated message handler (simplified, no excessive logging)
services.AddScoped<AuthenticatedHttpMessageHandler>();

 // ✅ Register typed clients using OAuth 2.1 user authentication
        services.AddHttpClient<IProductApiClientV1, ProductApiClientV1>(client =>
  {
    client.BaseAddress = new Uri(v1.BaseUrl);
  client.Timeout = TimeSpan.FromSeconds(v1.TimeoutSeconds);
    })
        .AddHttpMessageHandler<AuthenticatedHttpMessageHandler>();

   services.AddHttpClient<IProductApiClientV2, ProductApiClientV2>(client =>
    {
     client.BaseAddress = new Uri(v2.BaseUrl);
      client.Timeout = TimeSpan.FromSeconds(v2.TimeoutSeconds);
   })
     .AddHttpMessageHandler<AuthenticatedHttpMessageHandler>();

        // ✅ Customer API with user authentication
     services.AddHttpClient<ICustomerApiService, CustomerApiService>(client =>
        {
  client.BaseAddress = new Uri(customerConfig.BaseUrl);
  client.Timeout = TimeSpan.FromSeconds(customerConfig.TimeoutSeconds);
})
   .AddHttpMessageHandler<AuthenticatedHttpMessageHandler>();

        // ✅ Country API with user authentication  
        services.AddHttpClient<ICountryApiService, CountryApiService>(client =>
     {
     client.BaseAddress = new Uri(countryConfig.BaseUrl);
      client.Timeout = TimeSpan.FromSeconds(countryConfig.TimeoutSeconds);
     })
        .AddHttpMessageHandler<AuthenticatedHttpMessageHandler>();

  // Adapter/service registration
   services.AddScoped<IProductApiService, ProductApiService>();

   return services;
    }
}
