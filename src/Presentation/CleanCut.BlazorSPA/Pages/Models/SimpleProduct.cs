using System;
using System.Collections.Generic;

namespace CleanCut.BlazorSPA.Pages.Models;

public class SimpleProduct
{
    public Guid Id { get; set; }

    // Core
    public string Name { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ProductCategory Category { get; set; } = ProductCategory.Other;

    // Pricing & inventory
    public Money? Price { get; set; }
    public int? QuantityInStock { get; set; }
    public bool IsAvailable { get; set; } = true;
    public bool IsTaxable { get; set; } = true;

    // Physical properties
    public double? WeightKg { get; set; }
    public Dimensions? Size { get; set; }

    // Relations
    public Guid? SupplierId { get; set; }

    // Metadata
    public List<string> Tags { get; set; } = new();
    public double? Rating { get; set; }
    public DateTime? ReleaseDate { get; set; }
    public DateTime? DiscontinuedDate { get; set; }

    // Images / files
    public string? ImageFileName { get; set; }
    public long? ImageFileSize { get; set; }

    // Administrative
    public string Notes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Status helper
    public ProductStatus Status
    {
        get
        {
            if (!IsAvailable) return ProductStatus.Inactive;
            if (DiscontinuedDate.HasValue && DiscontinuedDate.Value <= DateTime.UtcNow) return ProductStatus.Discontinued;
            return ProductStatus.Active;
        }
    }
}

public record Money(decimal Amount, string Currency)
{
    public override string ToString() => $"{Amount:C2} {Currency}";
}

public class Dimensions
{
    public decimal? Length { get; set; }
    public decimal? Width { get; set; }
    public decimal? Height { get; set; }
}

public class SupplierInfo
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ContactEmail { get; set; }
}

public enum ProductCategory
{
    Unknown = 0,
    Electronics,
    Apparel,
    Grocery,
    Home,
    Tools,
    Toys,
    Books,
    Sports,
    Health,
    Other
}

public enum ProductStatus
{
    Unknown = 0,
    Active,
    Inactive,
    Discontinued
}
