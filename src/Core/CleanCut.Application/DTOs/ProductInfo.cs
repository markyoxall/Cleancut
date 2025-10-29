namespace CleanCut.Application.DTOs;

/// <summary>
/// Data Transfer Object for Product
/// </summary>
public class ProductInfo
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool IsAvailable { get; set; }

    // Renamed owner reference from UserId to CustomerId
    public Guid CustomerId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Optional customer information
    public CustomerInfo? Customer { get; set; }
}