namespace CleanCut.Application.DTOs;

public class CartItemInfo
{
    public Guid ProductId { get; set; }
    public string? Name { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
}
