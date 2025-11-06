using CleanCut.Domain.Common;
using CleanCut.Domain.Events;

namespace CleanCut.Domain.Entities;

/// <summary>
/// Represents an order in the system
/// </summary>
public class Order : BaseEntity
{
    private readonly List<OrderLineItem> _orderLineItems = [];

    public Guid CustomerId { get; private set; }
    public string OrderNumber { get; private set; } = string.Empty;
    public DateTime OrderDate { get; private set; }
    public OrderStatus Status { get; private set; }
    public decimal TotalAmount { get; private set; }
    public string ShippingAddress { get; private set; } = string.Empty;
    public string BillingAddress { get; private set; } = string.Empty;
    public string? Notes { get; private set; }

    // Navigation properties
    public Customer? Customer { get; private set; }
    public IReadOnlyCollection<OrderLineItem> OrderLineItems => _orderLineItems.AsReadOnly();

    // Private constructor for EF Core
    private Order() { }

    public Order(Guid customerId, string shippingAddress, string billingAddress, string? notes = null)
    {
        if (customerId == Guid.Empty)
            throw new ArgumentException("Customer ID cannot be empty", nameof(customerId));
        
        if (string.IsNullOrWhiteSpace(shippingAddress))
            throw new ArgumentException("Shipping address cannot be empty", nameof(shippingAddress));
        
        if (string.IsNullOrWhiteSpace(billingAddress))
            throw new ArgumentException("Billing address cannot be empty", nameof(billingAddress));

        CustomerId = customerId;
        OrderNumber = GenerateOrderNumber();
        OrderDate = DateTime.UtcNow;
        Status = OrderStatus.Pending;
        TotalAmount = 0;
        ShippingAddress = shippingAddress.Trim();
        BillingAddress = billingAddress.Trim();
        Notes = notes?.Trim();
        
        // Raise domain event
        AddDomainEvent(new OrderCreatedEvent(this));
    }

    public void AddLineItem(Guid productId, string productName, decimal unitPrice, int quantity)
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException($"Cannot modify order in {Status} status");
        
        if (productId == Guid.Empty)
            throw new ArgumentException("Product ID cannot be empty", nameof(productId));
        
        if (string.IsNullOrWhiteSpace(productName))
            throw new ArgumentException("Product name cannot be empty", nameof(productName));
        
        if (unitPrice < 0)
            throw new ArgumentException("Unit price cannot be negative", nameof(unitPrice));
        
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));

        // Check if product already exists in order
        var existingItem = _orderLineItems.FirstOrDefault(x => x.ProductId == productId);
        if (existingItem != null)
        {
            existingItem.UpdateQuantity(existingItem.Quantity + quantity);
        }
        else
        {
            var lineItem = new OrderLineItem(Id, productId, productName, unitPrice, quantity);
            _orderLineItems.Add(lineItem);
        }

        RecalculateTotalAmount();
        SetUpdatedAt();
        
        // Raise domain event
        AddDomainEvent(new OrderUpdatedEvent(this, "LineItems"));
    }

    public void RemoveLineItem(Guid productId)
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException($"Cannot modify order in {Status} status");

        var lineItem = _orderLineItems.FirstOrDefault(x => x.ProductId == productId);
        if (lineItem != null)
        {
            _orderLineItems.Remove(lineItem);
            RecalculateTotalAmount();
            SetUpdatedAt();
            
            // Raise domain event
            AddDomainEvent(new OrderUpdatedEvent(this, "LineItems"));
        }
    }

    public void UpdateLineItemQuantity(Guid productId, int newQuantity)
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException($"Cannot modify order in {Status} status");
        
        if (newQuantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(newQuantity));

        var lineItem = _orderLineItems.FirstOrDefault(x => x.ProductId == productId);
        if (lineItem == null)
            throw new InvalidOperationException($"Product {productId} not found in order");

        lineItem.UpdateQuantity(newQuantity);
        RecalculateTotalAmount();
        SetUpdatedAt();
        
        // Raise domain event
        AddDomainEvent(new OrderUpdatedEvent(this, "LineItems"));
    }

    public void UpdateShippingAddress(string shippingAddress)
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException($"Cannot modify shipping address for order in {Status} status");
        
        if (string.IsNullOrWhiteSpace(shippingAddress))
            throw new ArgumentException("Shipping address cannot be empty", nameof(shippingAddress));

        ShippingAddress = shippingAddress.Trim();
        SetUpdatedAt();
        
        // Raise domain event
        AddDomainEvent(new OrderUpdatedEvent(this, "ShippingAddress"));
    }

    public void UpdateBillingAddress(string billingAddress)
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException($"Cannot modify billing address for order in {Status} status");
        
        if (string.IsNullOrWhiteSpace(billingAddress))
            throw new ArgumentException("Billing address cannot be empty", nameof(billingAddress));

        BillingAddress = billingAddress.Trim();
        SetUpdatedAt();
        
        // Raise domain event
        AddDomainEvent(new OrderUpdatedEvent(this, "BillingAddress"));
    }

    public void UpdateNotes(string? notes)
    {
        Notes = notes?.Trim();
        SetUpdatedAt();
        
        // Raise domain event
        AddDomainEvent(new OrderUpdatedEvent(this, "Notes"));
    }

    public void ConfirmOrder()
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException($"Cannot confirm order in {Status} status");
        
        if (!_orderLineItems.Any())
            throw new InvalidOperationException("Cannot confirm order without line items");

        Status = OrderStatus.Confirmed;
        SetUpdatedAt();
        
        // Raise domain event
        AddDomainEvent(new OrderStatusChangedEvent(this, OrderStatus.Pending, OrderStatus.Confirmed));
    }

    public void ShipOrder()
    {
        if (Status != OrderStatus.Confirmed)
            throw new InvalidOperationException($"Cannot ship order in {Status} status");

        Status = OrderStatus.Shipped;
        SetUpdatedAt();
        
        // Raise domain event
        AddDomainEvent(new OrderStatusChangedEvent(this, OrderStatus.Confirmed, OrderStatus.Shipped));
    }

    public void DeliverOrder()
    {
        if (Status != OrderStatus.Shipped)
            throw new InvalidOperationException($"Cannot deliver order in {Status} status");

        Status = OrderStatus.Delivered;
        SetUpdatedAt();
        
        // Raise domain event
        AddDomainEvent(new OrderStatusChangedEvent(this, OrderStatus.Shipped, OrderStatus.Delivered));
    }

    public void CancelOrder()
    {
        if (Status == OrderStatus.Delivered)
            throw new InvalidOperationException("Cannot cancel delivered order");
        
        if (Status == OrderStatus.Cancelled)
            throw new InvalidOperationException("Order is already cancelled");

        var previousStatus = Status;
        Status = OrderStatus.Cancelled;
        SetUpdatedAt();
        
        // Raise domain event
        AddDomainEvent(new OrderStatusChangedEvent(this, previousStatus, OrderStatus.Cancelled));
    }

    public bool BelongsToCustomer(Guid customerId)
    {
        return CustomerId == customerId;
    }

    public int GetTotalItemCount()
    {
        return _orderLineItems.Sum(x => x.Quantity);
    }

    private void RecalculateTotalAmount()
    {
        TotalAmount = _orderLineItems.Sum(x => x.GetLineTotal());
    }

    private static string GenerateOrderNumber()
    {
        // Generate order number in format: ORD-YYYYMMDD-HHMMSS-XXXX
        var now = DateTime.UtcNow;
        var random = new Random();
        var randomSuffix = random.Next(1000, 9999);
        
        return $"ORD-{now:yyyyMMdd}-{now:HHmmss}-{randomSuffix}";
    }
}

/// <summary>
/// Order status enumeration
/// </summary>
public enum OrderStatus
{
    Pending = 0,
    Confirmed = 1,
    Shipped = 2,
    Delivered = 3,
    Cancelled = 4
}