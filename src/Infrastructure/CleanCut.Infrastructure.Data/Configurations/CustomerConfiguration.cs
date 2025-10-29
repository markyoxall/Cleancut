using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CleanCut.Domain.Entities;

namespace CleanCut.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework configuration for Customer entity
/// </summary>
public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");

        builder.HasKey(x => x.Id);

  builder.Property(x => x.Id)
  .ValueGeneratedNever(); // Using Domain-generated GUIDs

        builder.Property(x => x.FirstName)
      .IsRequired()
         .HasMaxLength(50);

        builder.Property(x => x.LastName)
     .IsRequired()
   .HasMaxLength(50);

        builder.Property(x => x.Email)
    .IsRequired()
            .HasMaxLength(255);

  builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
        .IsRequired();

    builder.Property(x => x.UpdatedAt)
            .IsRequired(false);

 // Ignore domain events - they should not be persisted
        builder.Ignore(x => x.DomainEvents);

 // Index for email uniqueness
        builder.HasIndex(x => x.Email)
     .IsUnique()
            .HasDatabaseName("IX_Customers_Email");

        // Index for active customers
        builder.HasIndex(x => x.IsActive)
            .HasDatabaseName("IX_Customers_IsActive");
    }
}