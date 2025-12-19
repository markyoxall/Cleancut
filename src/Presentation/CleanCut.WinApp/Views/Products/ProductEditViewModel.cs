using System;

namespace CleanCut.WinApp.Views.Products;

/// <summary>
/// View model for product edit form
/// </summary>
public class ProductEditViewModel
{
    public Guid? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool IsAvailable { get; set; } = true;
    public Guid UserId { get; set; }
}
