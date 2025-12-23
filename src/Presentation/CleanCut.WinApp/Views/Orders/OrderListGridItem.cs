using System;

namespace CleanCut.WinApp.Views.Orders
{
    /// <summary>
    /// Presentation model for displaying orders in the grid.
    /// </summary>
    public class OrderListGridItem
    {
        public Guid Id { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime? LastModified { get; set; }
        public bool IsActive { get; set; }
    }
}
