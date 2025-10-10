using CleanCut.Application.DTOs;
using System.ComponentModel.DataAnnotations;

namespace CleanCut.WebApp.Models.Users;

/// <summary>
/// View model for displaying list of users
/// </summary>
public class UserListViewModel
{
    public List<UserDto> Users { get; set; } = new();
    public string? SearchTerm { get; set; }
    public bool? IsActiveFilter { get; set; }
    public int TotalUsers { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool IsSuccess { get; set; }
}

/// <summary>
/// View model for creating/editing users
/// </summary>
public class UserEditViewModel
{
    public Guid? Id { get; set; }
    
    [Required(ErrorMessage = "First name is required")]
    [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
    [Display(Name = "First Name")]
    public string FirstName { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Last name is required")]
    [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
    [Display(Name = "Last Name")]
    public string LastName { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address")]
    [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
    public string Email { get; set; } = string.Empty;
    
    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;
    
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    public bool IsEditMode => Id.HasValue;
}

/// <summary>
/// View model for user details display
/// </summary>
public class UserDetailsViewModel
{
    public UserDto User { get; set; } = new();
    public List<ProductDto> UserProducts { get; set; } = new();
    public int TotalProducts { get; set; }
}