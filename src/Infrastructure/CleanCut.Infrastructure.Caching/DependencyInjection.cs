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
            try
            {
                // Add Redis connection with timeout configuration
                services.AddSingleton<IConnectionMultiplexer>(provider =>
                {
                    var configurationOptions = ConfigurationOptions.Parse(redisConnectionString);
                    configurationOptions.ConnectTimeout = 2000; // 2 seconds instead of 5
                    configurationOptions.SyncTimeout = 2000;    // 2 seconds instead of 5
                    configurationOptions.AbortOnConnectFail = false; // Don't abort if Redis is down
                    return ConnectionMultiplexer.Connect(configurationOptions);
                });

                // Add Redis distributed cache with shorter timeouts
                services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = redisConnectionString;
                    options.InstanceName = "CleanCut";
                    // Configure connection options
                    options.ConfigurationOptions = ConfigurationOptions.Parse(redisConnectionString);
                    options.ConfigurationOptions.ConnectTimeout = 2000; // 2 seconds
                    options.ConfigurationOptions.SyncTimeout = 2000;    // 2 seconds  
                    options.ConfigurationOptions.AbortOnConnectFail = false;
                });

                // Register cache service
                services.AddScoped<ICacheService, RedisCacheService>();
                
                Console.WriteLine("? Redis caching configured successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"?? Redis configuration failed, falling back to in-memory: {ex.Message}");
                
                // Fall back to in-memory cache if Redis configuration fails
                services.AddMemoryCache();
                services.AddDistributedMemoryCache(); // This is the correct way to add IDistributedCache
                services.AddScoped<ICacheService, MemoryCacheService>();
            }
        }
        else
        {
            Console.WriteLine("?? No Redis connection string found, using in-memory cache");
            
            // Fall back to in-memory cache if Redis is not configured
            services.AddMemoryCache();
            services.AddDistributedMemoryCache(); // This is the correct way to add IDistributedCache
            services.AddScoped<ICacheService, MemoryCacheService>();
        }

        return services;
    }
}