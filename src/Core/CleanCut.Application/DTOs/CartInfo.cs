namespace CleanCut.Application.DTOs;

public class CartInfo
{
    public Guid Id { get; set; }
    public Guid OwnerId { get; set; }
    public List<CartItemDto> Items { get; set; } = new();
    public decimal Total { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
