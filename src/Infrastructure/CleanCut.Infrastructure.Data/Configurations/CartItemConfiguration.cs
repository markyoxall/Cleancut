using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CleanCut.Domain.Entities;

namespace CleanCut.Infrastructure.Data.Configurations;

public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.ToTable("CartItems");
        builder.HasKey(ci => ci.Id);
        builder.Property(ci => ci.ProductId).IsRequired();
        builder.Property(ci => ci.ProductName).HasMaxLength(200).IsRequired();
        builder.Property(ci => ci.UnitPrice).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(ci => ci.Quantity).IsRequired();
        builder.Property(ci => ci.CreatedAt).IsRequired();
        builder.Property(ci => ci.UpdatedAt);

        builder.HasIndex(ci => ci.CartId);
        builder.HasIndex(ci => ci.ProductId);
    }
}
