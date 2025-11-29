namespace CleanCut.Application.DTOs;

public class CartItemDto
{
    public Guid ProductId { get; set; }
    public string? Name { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
}
