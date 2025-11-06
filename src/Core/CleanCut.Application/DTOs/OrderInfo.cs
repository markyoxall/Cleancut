using CleanCut.Domain.Entities;

namespace CleanCut.Application.DTOs;

/// <summary>
/// Data Transfer Object for Order
/// </summary>
public class OrderInfo
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public OrderStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string ShippingAddress { get; set; } = string.Empty;
    public string BillingAddress { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public int TotalItemCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Customer information
    public string? CustomerName { get; set; }
    public string? CustomerEmail { get; set; }

    // Order line items
    public List<OrderLineItemInfo> OrderLineItems { get; set; } = [];
}