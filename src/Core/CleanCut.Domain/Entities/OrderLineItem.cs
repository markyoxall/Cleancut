using CleanCut.Domain.Common;

namespace CleanCut.Domain.Entities;

/// <summary>
/// Represents a line item within an order
/// </summary>
public class OrderLineItem : BaseEntity
{
    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; } = string.Empty;
    public decimal UnitPrice { get; private set; }
    public int Quantity { get; private set; }

    // Navigation properties
    public Order? Order { get; private set; }
    public Product? Product { get; private set; }

    // Private constructor for EF Core
    private OrderLineItem() { }

    public OrderLineItem(Guid orderId, Guid productId, string productName, decimal unitPrice, int quantity)
    {
        if (orderId == Guid.Empty)
            throw new ArgumentException("Order ID cannot be empty", nameof(orderId));
        
        if (productId == Guid.Empty)
            throw new ArgumentException("Product ID cannot be empty", nameof(productId));
        
        if (string.IsNullOrWhiteSpace(productName))
            throw new ArgumentException("Product name cannot be empty", nameof(productName));
        
        if (unitPrice < 0)
            throw new ArgumentException("Unit price cannot be negative", nameof(unitPrice));
        
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));

        OrderId = orderId;
        ProductId = productId;
        ProductName = productName.Trim();
        UnitPrice = unitPrice;
        Quantity = quantity;
    }

    public void UpdateQuantity(int newQuantity)
    {
        if (newQuantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(newQuantity));

        Quantity = newQuantity;
        SetUpdatedAt();
    }

    public void UpdateUnitPrice(decimal newUnitPrice)
    {
        if (newUnitPrice < 0)
            throw new ArgumentException("Unit price cannot be negative", nameof(newUnitPrice));

        UnitPrice = newUnitPrice;
        SetUpdatedAt();
    }

    public decimal GetLineTotal()
    {
        return UnitPrice * Quantity;
    }

    public bool IsForProduct(Guid productId)
    {
        return ProductId == productId;
    }
}