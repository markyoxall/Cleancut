namespace CleanCut.BackgroundService.Models;

/// <summary>
/// Product information DTO for API responses
/// </summary>
public class ProductInfo
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool IsAvailable { get; set; }
    public Guid CustomerId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // For CSV export, we can add computed properties
    public string Status => IsAvailable ? "Available" : "Unavailable";
    public string PriceFormatted => Price.ToString("C2");
    public string CreatedDate => CreatedAt.ToString("yyyy-MM-dd");
}