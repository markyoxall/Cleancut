using System;
using CleanCut.BlazorWebApp.Services;
using CleanCut.BlazorWebApp.Services.HttpClients;
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

        // Register typed clients using configuration values
        services.AddHttpClient<IProductApiClientV1, ProductApiClientV1>(client =>
        {
            client.BaseAddress = new Uri(v1.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(v1.TimeoutSeconds);
        });

        services.AddHttpClient<IProductApiClientV2, ProductApiClientV2>(client =>
        {
            client.BaseAddress = new Uri(v2.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(v2.TimeoutSeconds);
        });

        // Adapter/service registration
        services.AddScoped<IProductApiService, ProductApiService>();

        return services;
    }
}
