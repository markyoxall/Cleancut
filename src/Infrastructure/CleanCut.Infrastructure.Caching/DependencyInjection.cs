using CleanCut.Infrastructure.Caching.Abstractions;
using CleanCut.Infrastructure.Caching.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
        var useRedis = !string.IsNullOrEmpty(redisConnectionString);
        
        if (useRedis)
        {
            try
            {
                // Test Redis connection before configuring
                var testOptions = ConfigurationOptions.Parse(redisConnectionString);
                testOptions.ConnectTimeout = 1000; // 1 second test timeout
                testOptions.SyncTimeout = 1000;
                testOptions.AbortOnConnectFail = true; // Fail fast for testing
                
                using var testConnection = ConnectionMultiplexer.Connect(testOptions);
                var testDatabase = testConnection.GetDatabase();
                testDatabase.Ping(); // Test connection
                
                // If we get here, Redis is available
                services.AddSingleton<IConnectionMultiplexer>(provider =>
                {
                    var configurationOptions = ConfigurationOptions.Parse(redisConnectionString);
                    configurationOptions.ConnectTimeout = 5000;
                    configurationOptions.SyncTimeout = 5000;
                    configurationOptions.AbortOnConnectFail = false;
                    return ConnectionMultiplexer.Connect(configurationOptions);
                });

                services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = redisConnectionString;
                    options.InstanceName = "CleanCut";
                });

                services.AddScoped<ICacheService, RedisCacheService>();
                
                Console.WriteLine("? Redis caching configured successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"?? Redis connection failed, falling back to in-memory cache: {ex.Message}");
                ConfigureInMemoryCache(services);
            }
        }
        else
        {
            Console.WriteLine("?? No Redis connection string found, using in-memory cache");
            ConfigureInMemoryCache(services);
        }

        return services;
    }
    
    private static void ConfigureInMemoryCache(IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddDistributedMemoryCache();
        services.AddScoped<ICacheService, MemoryCacheService>();
    }
}
