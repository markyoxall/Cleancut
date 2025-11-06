using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CleanCut.Domain.Entities;

namespace CleanCut.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework configuration for Order entity
/// </summary>
public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        // Table configuration
        builder.ToTable("Orders");

        // Primary key
        builder.HasKey(o => o.Id);

        // Properties
        builder.Property(o => o.Id)
            .IsRequired()
            .ValueGeneratedNever(); // We're using our own GUID generation

        builder.Property(o => o.CustomerId)
            .IsRequired();

        builder.Property(o => o.OrderNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(o => o.OrderDate)
            .IsRequired();

        builder.Property(o => o.Status)
            .IsRequired()
            .HasConversion<int>(); // Store enum as int

        builder.Property(o => o.TotalAmount)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(o => o.ShippingAddress)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(o => o.BillingAddress)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(o => o.Notes)
            .HasMaxLength(1000);

        builder.Property(o => o.CreatedAt)
            .IsRequired();

        builder.Property(o => o.UpdatedAt);

        // Indexes
        builder.HasIndex(o => o.OrderNumber)
            .IsUnique()
            .HasDatabaseName("IX_Orders_OrderNumber");

        builder.HasIndex(o => o.CustomerId)
            .HasDatabaseName("IX_Orders_CustomerId");

        builder.HasIndex(o => o.Status)
            .HasDatabaseName("IX_Orders_Status");

        builder.HasIndex(o => o.OrderDate)
            .HasDatabaseName("IX_Orders_OrderDate");

        // Relationships
        builder.HasOne(o => o.Customer)
            .WithMany()
            .HasForeignKey(o => o.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(o => o.OrderLineItems)
            .WithOne(oli => oli.Order)
            .HasForeignKey(oli => oli.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure private backing field for OrderLineItems collection
        builder.Metadata.FindNavigation(nameof(Order.OrderLineItems))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        // Ignore domain events
        builder.Ignore(o => o.DomainEvents);
    }
}