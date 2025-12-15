using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CleanCut.Domain.Entities;

namespace CleanCut.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework configuration for OrderLineItem entity
/// </summary>
public class OrderLineItemConfiguration : IEntityTypeConfiguration<OrderLineItem>
{
    public void Configure(EntityTypeBuilder<OrderLineItem> builder)
    {
        // Table configuration
        builder.ToTable("OrderLineItems");

        // Primary key
        builder.HasKey(oli => oli.Id);

        // Properties
        builder.Property(oli => oli.Id)
            .IsRequired()
            .ValueGeneratedNever(); // We're using our own GUID generation

        builder.Property(oli => oli.OrderId)
            .IsRequired();

        builder.Property(oli => oli.ProductId)
            .IsRequired();

        builder.Property(oli => oli.ProductName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(oli => oli.UnitPrice)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(oli => oli.Quantity)
            .IsRequired();

        builder.Property(oli => oli.CreatedAt)
            .IsRequired();

        builder.Property(oli => oli.UpdatedAt);

        // Indexes
        builder.HasIndex(oli => oli.OrderId)
            .HasDatabaseName("IX_OrderLineItems_OrderId");

        builder.HasIndex(oli => oli.ProductId)
            .HasDatabaseName("IX_OrderLineItems_ProductId");

        // Relationships
        builder.HasOne(oli => oli.Order)
            .WithMany(o => o.OrderLineItems)
            .HasForeignKey(oli => oli.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(oli => oli.Product)
            .WithMany()
            .HasForeignKey(oli => oli.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // Ignore domain events
        builder.Ignore(oli => oli.DomainEvents);
    }
}