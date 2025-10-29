using System;

namespace CleanCut.BlazorWebApp.Services;

/// <summary>
/// Request model sent from UI to Product API service when creating a product.
/// Kept local to presentation layer to avoid coupling with API command types.
/// </summary>
public class CreateProductRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public Guid CustomerId { get; set; }
}

/// <summary>
/// Request model for updating a product.
/// </summary>
public class UpdateProductRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
}