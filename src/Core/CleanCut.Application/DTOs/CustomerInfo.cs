namespace CleanCut.Application.DTOs;

/// <summary>
/// Data Transfer Object for User
/// </summary>
public class CustomerInfo
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Gets the full name (for consistency with domain entity)
    /// </summary>
    public string GetFullName() => FullName;
}