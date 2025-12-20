using System;
using System.Collections.Generic;

namespace CleanCut.WinApp.Views.Orders;

public interface IOrderLineItemListView : MVP.IView
{
    event EventHandler? AddLineItemRequested;
    event EventHandler<Guid>? EditLineItemRequested;
    event EventHandler<Guid>? DeleteLineItemRequested;
    event EventHandler? RefreshRequested;
    void DisplayLineItems(IEnumerable<CleanCut.Application.DTOs.OrderLineItemInfo> lineItems);
    void ClearLineItems();
    Guid? GetSelectedLineItemId();
}
