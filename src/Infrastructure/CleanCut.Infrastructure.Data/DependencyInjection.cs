using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CleanCut.Domain.Repositories;
using CleanCut.Infrastructure.Data.Context;
using CleanCut.Infrastructure.Data.Repositories;
using CleanCut.Infrastructure.Data.UnitOfWork;

namespace CleanCut.Infrastructure.Data;

/// <summary>
/// Dependency injection configuration for Data Infrastructure layer
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddDataInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Add Entity Framework DbContext
        services.AddDbContext<CleanCutDbContext>(options =>
        {
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(CleanCutDbContext).Assembly.FullName));

            // Enable sensitive data logging in development
            var enableSensitiveDataLogging = configuration.GetSection("EnableSensitiveDataLogging").Get<bool>();
            if (enableSensitiveDataLogging)
            {
                options.EnableSensitiveDataLogging();
            }

            // Enable detailed errors in development
            var enableDetailedErrors = configuration.GetSection("EnableDetailedErrors").Get<bool>();
            if (enableDetailedErrors)
            {
                options.EnableDetailedErrors();
            }
        });

        // Register repositories
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICountryRepository, CountryRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IOrderLineItemRepository, OrderLineItemRepository>();
        services.AddScoped<ICartRepository, CartRepository>();
        services.AddScoped<CleanCut.Application.Common.Interfaces.IIdempotencyRepository, IdempotencyRepository>();

        // Register Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork.UnitOfWork>();

        return services;
    }
}
