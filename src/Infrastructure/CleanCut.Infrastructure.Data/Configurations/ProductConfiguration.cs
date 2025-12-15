using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CleanCut.Domain.Entities;

namespace CleanCut.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework configuration for Product entity
/// </summary>
public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever(); // Using Domain-generated GUIDs

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.Price)
            .IsRequired()
            .HasColumnType("decimal(18,2)"); // Specify precision for money

        builder.Property(x => x.IsAvailable)
            .IsRequired();

        builder.Property(x => x.CustomerId)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired(false);

        // Ignore domain events - they should not be persisted
        builder.Ignore(x => x.DomainEvents);

        // Indexes
        builder.HasIndex(x => x.CustomerId)
            .HasDatabaseName("IX_Products_UserId");

        builder.HasIndex(x => x.IsAvailable)
            .HasDatabaseName("IX_Products_IsAvailable");

        builder.HasIndex(x => new { x.CustomerId, x.IsAvailable })
            .HasDatabaseName("IX_Products_UserId_IsAvailable");
    }
}