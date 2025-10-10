using CleanCut.Domain.Common;
using CleanCut.Domain.Events;
using System.Net.Mail;

namespace CleanCut.Domain.Entities;

/// <summary>
/// Represents a user in the system
/// </summary>
public class User : BaseEntity
{
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;

    // Private constructor for EF Core
    private User() { }

    public User(string firstName, string lastName, string email)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name cannot be empty", nameof(firstName));
        
        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name cannot be empty", nameof(lastName));
        
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty", nameof(email));
        
        if (!IsValidEmail(email))
            throw new ArgumentException("Invalid email format", nameof(email));

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        Email = email.Trim().ToLowerInvariant();
        
        // Raise domain event
        AddDomainEvent(new UserCreatedEvent(this));
    }

    public string GetFullName() => $"{FirstName} {LastName}";

    public void UpdateName(string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name cannot be empty", nameof(firstName));
        
        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name cannot be empty", nameof(lastName));

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        SetUpdatedAt();
        
        // Raise domain event
        AddDomainEvent(new UserUpdatedEvent(this, "Name"));
    }

    public void UpdateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty", nameof(email));
        
        if (!IsValidEmail(email))
            throw new ArgumentException("Invalid email format", nameof(email));

        Email = email.Trim().ToLowerInvariant();
        SetUpdatedAt();
        
        // Raise domain event
        AddDomainEvent(new UserUpdatedEvent(this, "Email"));
    }

    public void Deactivate()
    {
        IsActive = false;
        SetUpdatedAt();
        
        // Raise domain event
        AddDomainEvent(new UserUpdatedEvent(this, "Status"));
    }

    public void Activate()
    {
        IsActive = true;
        SetUpdatedAt();
        
        // Raise domain event
        AddDomainEvent(new UserUpdatedEvent(this, "Status"));
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}