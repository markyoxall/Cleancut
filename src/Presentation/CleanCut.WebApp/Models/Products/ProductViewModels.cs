using CleanCut.Application.DTOs;
using System.ComponentModel.DataAnnotations;

namespace CleanCut.WebApp.Models.Products;

/// <summary>
/// View model for displaying list of products
/// </summary>
public class ProductListViewModel
{
    public List<ProductDto> Products { get; set; } = new();
    public List<UserDto> Users { get; set; } = new();
    public Guid? SelectedUserId { get; set; }
    public string? SearchTerm { get; set; }
    public bool? IsAvailableFilter { get; set; }
    public int TotalProducts { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool IsSuccess { get; set; }
}

/// <summary>
/// View model for creating/editing products
/// </summary>
public class ProductEditViewModel
{
    public Guid? Id { get; set; }
    
    [Required(ErrorMessage = "Product name is required")]
    [StringLength(100, ErrorMessage = "Product name cannot exceed 100 characters")]
    [Display(Name = "Product Name")]
    public string Name { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Description is required")]
    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    [DataType(DataType.MultilineText)]
    public string Description { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Price is required")]
    [Range(0.01, 999999.99, ErrorMessage = "Price must be between $0.01 and $999,999.99")]
    [DataType(DataType.Currency)]
    [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
    public decimal Price { get; set; }
    
    [Display(Name = "Available")]
    public bool IsAvailable { get; set; } = true;
    
    [Required(ErrorMessage = "Please select a user")]
    [Display(Name = "Owner")]
    public Guid UserId { get; set; }
    
    public List<UserDto> AvailableUsers { get; set; } = new();
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    public bool IsEditMode => Id.HasValue;
}

/// <summary>
/// View model for product details display
/// </summary>
public class ProductDetailsViewModel
{
    public ProductDto Product { get; set; } = new();
    public UserDto? Owner { get; set; }
}