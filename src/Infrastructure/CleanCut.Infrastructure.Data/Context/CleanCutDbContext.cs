using Microsoft.EntityFrameworkCore;
using CleanCut.Domain.Entities;
using CleanCut.Domain.Common;
using CleanCut.Domain.Events;
using CleanCut.Infrastructure.Data.Entities;

namespace CleanCut.Infrastructure.Data.Context;

/// <summary>
/// Main Entity Framework DbContext for the CleanCut application
/// </summary>
public class CleanCutDbContext : DbContext
{
    public CleanCutDbContext(DbContextOptions<CleanCutDbContext> options) : base(options)
    {
    }

    public DbSet<Customer> Customers { get; set; } = null!;
    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<Country> Countries { get; set; } = null!;
    public DbSet<Order> Orders { get; set; } = null!;
    public DbSet<OrderLineItem> OrderLineItems { get; set; } = null!;
    public DbSet<Cart> Carts { get; set; } = null!;
    public DbSet<CartItem> CartItems { get; set; } = null!;
    public DbSet<ExportRecord> ExportRecords { get; set; } = null!;
    public DbSet<UserPreferenceEntity> UserPreferences { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Ignore all domain event types - they should not be persisted
        modelBuilder.Ignore<DomainEvent>();
        modelBuilder.Ignore<CustomerCreatedEvent>();
        modelBuilder.Ignore<CustomerUpdatedEvent>();
        modelBuilder.Ignore<OrderCreatedEvent>();
        modelBuilder.Ignore<OrderUpdatedEvent>();
        modelBuilder.Ignore<OrderStatusChangedEvent>();
        
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
        
        // Configure idempotency record
        modelBuilder.Entity<IdempotencyRecord>(b =>
        {
            b.HasKey(e => e.Key);
            b.Property(e => e.Key).HasMaxLength(200).IsRequired();
            b.Property(e => e.RequestHash).HasMaxLength(1000);
            b.Property(e => e.ResponseStatus);
            b.Property(e => e.ResponsePayload);
            b.Property(e => e.ResponseHeaders);
            b.HasIndex(e => e.CreatedAt);
        });

        // Configure export record
        modelBuilder.Entity<ExportRecord>(b =>
        {
            b.HasKey(e => e.Id);
            b.Property(e => e.OrderId).IsRequired();
            b.Property(e => e.ExportType).HasMaxLength(100);
            b.HasIndex(e => e.OrderId);
            b.HasIndex(e => e.ExportedAt);
        });
        
        // Configure user preferences table
        modelBuilder.Entity<UserPreferenceEntity>(b =>
        {
            b.HasKey(e => e.ModuleName);
            b.Property(e => e.PayloadJson).IsRequired();
            b.Property(e => e.CreatedAt).IsRequired();
            b.Property(e => e.UpdatedAt).IsRequired();
        });
        
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
