using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CleanCut.Infrastructure.BackgroundServices.Workers;
using CleanCut.Infrastructure.Shared;

namespace CleanCut.Infrastructure.BackgroundServices.Extensions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddBackgroundServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Register shared infrastructure used by background services
        services.AddSharedInfrastructure(configuration);

        // Register hosted workers
        services.AddHostedService<OrderEmailWorker>();
        services.AddHostedService<RabbitMqRetryWorker>();

        return services;
    }
}
