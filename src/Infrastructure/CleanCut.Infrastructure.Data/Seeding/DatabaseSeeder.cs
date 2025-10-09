using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using CleanCut.Domain.Entities;
using CleanCut.Infrastructure.Data.Context;

namespace CleanCut.Infrastructure.Data.Seeding;

/// <summary>
/// Database seeder for development data
/// </summary>
public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CleanCutDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<CleanCutDbContext>>();

        try
        {
            // Ensure database is created
            await context.Database.EnsureCreatedAsync();

            // Check if data already exists
            if (await context.Users.AnyAsync())
            {
                logger.LogInformation("Database already seeded");
                return;
            }

            logger.LogInformation("Seeding database with initial data");

            // Seed Users
            var users = new List<User>
            {
                new("John", "Doe", "john.doe@example.com"),
                new("Jane", "Smith", "jane.smith@example.com"),
                new("Bob", "Johnson", "bob.johnson@example.com")
            };

            await context.Users.AddRangeAsync(users);
            await context.SaveChangesAsync();

            // Seed Products
            var products = new List<Product>
            {
                new("Laptop", "High-performance laptop for development", 1299.99m, users[0].Id),
                new("Wireless Mouse", "Ergonomic wireless mouse", 49.99m, users[0].Id),
                new("Mechanical Keyboard", "RGB mechanical keyboard", 129.99m, users[1].Id),
                new("Monitor", "4K UHD monitor 27 inch", 399.99m, users[1].Id),
                new("Desk Chair", "Comfortable office chair", 249.99m, users[2].Id)
            };

            await context.Products.AddRangeAsync(products);
            await context.SaveChangesAsync();

            logger.LogInformation("Database seeded successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database");
            throw;
        }
    }
}