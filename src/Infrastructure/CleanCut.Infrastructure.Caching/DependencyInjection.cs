using CleanCut.Infrastructure.Caching.Abstractions;
using CleanCut.Infrastructure.Caching.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace CleanCut.Infrastructure.Caching;

/// <summary>
/// Dependency injection configuration for Caching Infrastructure layer
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddCachingInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Get Redis connection string
        var redisConnectionString = configuration.GetConnectionString("Redis");
        
        if (!string.IsNullOrEmpty(redisConnectionString))
        {
            // Add Redis connection
            services.AddSingleton<IConnectionMultiplexer>(provider =>
            {
                var configuration = ConfigurationOptions.Parse(redisConnectionString);
                return ConnectionMultiplexer.Connect(configuration);
            });

            // Add Redis distributed cache
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnectionString;
                options.InstanceName = "CleanCut";
            });

            // Register cache service
            services.AddScoped<ICacheService, RedisCacheService>();
        }
        else
        {
            // Fall back to in-memory cache if Redis is not configured
            services.AddMemoryCache();
            services.AddScoped<ICacheService, MemoryCacheService>();
        }

        return services;
    }
}