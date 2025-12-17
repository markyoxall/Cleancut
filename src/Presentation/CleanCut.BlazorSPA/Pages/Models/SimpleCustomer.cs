using System;
using System.Collections.Generic;

namespace CleanCut.BlazorSPA.Pages.Models;

public class SimpleCustomer
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    // Additional demo properties
    public int? Age { get; set; }
    public DateTime? BirthDate { get; set; }
    public decimal? Balance { get; set; }
    public double? Rating { get; set; }
    public Gender Gender { get; set; }
    public ContactMethod PreferredContact { get; set; }
    public string? PhoneNumber { get; set; }
    public Uri? Website { get; set; }
    public string? PostalCode { get; set; }
    public DateTime? MemberSince { get; set; }
    public DateTime? LastLogin { get; set; }
    public TimeSpan? PreferredContactTime { get; set; }
    public string Notes { get; set; } = string.Empty;

    // Preferences
    public bool IsSubscribed { get; set; }
    public bool IsPremium { get; set; }
    public string Country { get; set; } = string.Empty;
    public string? FavoriteColor { get; set; }

    // Simple list of tags
    public List<string> Tags { get; set; } = new();

    // File metadata (for InputFile demo)
    public string? UploadedFileName { get; set; }
    public long? UploadedFileSize { get; set; }
}

public enum Gender
{
    Unknown = 0,
    Male,
    Female,
    Other
}

public enum ContactMethod
{
    Unknown = 0,
    Email,
    Phone,
    SMS
}
