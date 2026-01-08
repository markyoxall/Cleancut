namespace CleanCut.Application.DTOs;

/// <summary>
/// Data Transfer Object for Customer
/// </summary>
public record CustomerInfo
{
    public Guid Id { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public string FullName { get; init; } = string.Empty;
}
