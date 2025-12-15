using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using CleanCut.Application.Behaviors;
using AutoMapper;

namespace CleanCut.Application;

/// <summary>
/// Dependency injection configuration for Application layer
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, bool includeIntegrationBehaviors = true)
    {
        // Add MediatR
        services.AddMediatR(cfg => 
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());

            // Add behaviors
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            cfg.AddOpenBehavior(typeof(CachingBehavior<,>));
            cfg.AddOpenBehavior(typeof(DomainEventDispatcherBehavior<,>));
            if (includeIntegrationBehaviors)
            {
                cfg.AddOpenBehavior(typeof(IdempotencyBehavior<,>));
                // Add RabbitMQ publishing behavior near the end so handlers can return DTOs
                cfg.AddOpenBehavior(typeof(RabbitMqPublishingBehavior<,>));
            }
        });

        // Configure AutoMapper via the Microsoft DI extensions
        var assemblies = new[] { Assembly.GetExecutingAssembly() };
        // Use the AddAutoMapper overload that accepts a configuration action so we can
        // call AddMaps with the assemblies containing profiles.
        services.AddAutoMapper(cfg => cfg.AddMaps(assemblies));

        // Add FluentValidation
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        return services;
    }
}
