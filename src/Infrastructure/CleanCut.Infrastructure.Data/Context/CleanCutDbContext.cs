using Microsoft.EntityFrameworkCore;
using CleanCut.Domain.Entities;
using CleanCut.Domain.Common;
using CleanCut.Domain.Events;

namespace CleanCut.Infrastructure.Data.Context;

/// <summary>
/// Main Entity Framework DbContext for the CleanCut application
/// </summary>
public class CleanCutDbContext : DbContext
{
    public CleanCutDbContext(DbContextOptions<CleanCutDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<Country> Countries { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Ignore all domain event types - they should not be persisted
        modelBuilder.Ignore<DomainEvent>();
        modelBuilder.Ignore<UserCreatedEvent>();
        modelBuilder.Ignore<UserUpdatedEvent>();
        
        // Configure BaseEntity to ignore domain events collection for all entities
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .Ignore(nameof(BaseEntity.DomainEvents));
            }
        }

        // Apply all configurations from the current assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CleanCutDbContext).Assembly);

        // Additional global configurations can be added here
        ConfigureGlobalSettings(modelBuilder);
    }

    private static void ConfigureGlobalSettings(ModelBuilder modelBuilder)
    {
        // Configure decimal precision globally for money-related properties
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var decimalProperties = entityType.ClrType.GetProperties()
                .Where(p => p.PropertyType == typeof(decimal) || p.PropertyType == typeof(decimal?));

            foreach (var property in decimalProperties)
            {
                if (property.Name.ToLowerInvariant().Contains("price") || 
                    property.Name.ToLowerInvariant().Contains("amount") ||
                    property.Name.ToLowerInvariant().Contains("cost"))
                {
                    modelBuilder.Entity(entityType.Name).Property(property.Name)
                        .HasColumnType("decimal(18,2)");
                }
            }
        }
    }
}