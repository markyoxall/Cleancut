using CleanCut.Domain.Common;

namespace CleanCut.Domain.Entities;

public class CartItem : BaseEntity
{
    public Guid CartId { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; } = string.Empty;
    public decimal UnitPrice { get; private set; }
    public int Quantity { get; private set; }

    private CartItem() { }

    public CartItem(Guid cartId, Guid productId, string productName, decimal unitPrice, int quantity)
    {
        if (cartId == Guid.Empty) throw new ArgumentException("CartId cannot be empty", nameof(cartId));
        if (productId == Guid.Empty) throw new ArgumentException("ProductId cannot be empty", nameof(productId));
        if (string.IsNullOrWhiteSpace(productName)) throw new ArgumentException("ProductName cannot be empty", nameof(productName));
        if (unitPrice < 0) throw new ArgumentException("UnitPrice cannot be negative", nameof(unitPrice));
        if (quantity <= 0) throw new ArgumentException("Quantity must be positive", nameof(quantity));

        CartId = cartId;
        ProductId = productId;
        ProductName = productName;
        UnitPrice = unitPrice;
        Quantity = quantity;
    }

    public void UpdateQuantity(int newQuantity)
    {
        if (newQuantity <= 0) throw new ArgumentException("Quantity must be positive", nameof(newQuantity));
        Quantity = newQuantity;
        SetUpdatedAt();
    }
}
