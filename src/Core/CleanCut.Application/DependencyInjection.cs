using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using CleanCut.Application.Behaviors;

namespace CleanCut.Application;

/// <summary>
/// Dependency injection configuration for Application layer
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Add MediatR
        services.AddMediatR(cfg => 
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            
            // Add behaviors
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            cfg.AddOpenBehavior(typeof(CachingBehavior<,>));
            cfg.AddOpenBehavior(typeof(DomainEventDispatcherBehavior<,>));
        });

        // Add AutoMapper
        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        // Add FluentValidation
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        return services;
    }
}