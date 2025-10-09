using CleanCut.Domain.Common;

namespace CleanCut.Domain.Entities;

/// <summary>
/// Represents a product in the system
/// </summary>
public class Product : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public bool IsAvailable { get; private set; } = true;
    public Guid UserId { get; private set; }

    // Navigation property
    public User? User { get; private set; }

    // Private constructor for EF Core
    private Product() { }

    public Product(string name, string description, decimal price, Guid userId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Product name cannot be empty", nameof(name));
        
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Product description cannot be empty", nameof(description));
        
        if (price < 0)
            throw new ArgumentException("Price cannot be negative", nameof(price));
        
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        Name = name.Trim();
        Description = description.Trim();
        Price = price;
        UserId = userId;
    }

    public void UpdateDetails(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Product name cannot be empty", nameof(name));
        
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Product description cannot be empty", nameof(description));

        Name = name.Trim();
        Description = description.Trim();
        SetUpdatedAt();
    }

    public void UpdatePrice(decimal price)
    {
        if (price < 0)
            throw new ArgumentException("Price cannot be negative", nameof(price));

        Price = price;
        SetUpdatedAt();
    }

    public void MakeAvailable()
    {
        IsAvailable = true;
        SetUpdatedAt();
    }

    public void MakeUnavailable()
    {
        IsAvailable = false;
        SetUpdatedAt();
    }

    public bool BelongsToUser(Guid userId)
    {
        return UserId == userId;
    }
}