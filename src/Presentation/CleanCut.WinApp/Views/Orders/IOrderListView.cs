using System;
using System.Collections.Generic;

namespace CleanCut.WinApp.Views.Orders;

/// <summary>
/// View interface for displaying and interacting with the order list grid.
/// </summary>
public interface IOrderListView : MVP.IView
{
    event EventHandler? AddOrderRequested;
    event EventHandler<Guid>? EditOrderRequested;
    event EventHandler<Guid>? DeleteOrderRequested;
    event EventHandler? RefreshRequested;
    event EventHandler<Guid>? ViewOrdersByCustomerRequested;
    event EventHandler<Guid>? ViewOrderLineItemsRequested;
    void DisplayOrders(IEnumerable<OrderListGridItem> orders);
    void ClearOrders();
    Guid? GetSelectedOrderId();
    void SetAvailableCustomers(IEnumerable<CleanCut.Application.DTOs.CustomerInfo> customers);
}
