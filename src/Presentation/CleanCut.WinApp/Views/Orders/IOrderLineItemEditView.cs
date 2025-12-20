using System;
using System.Collections.Generic;

namespace CleanCut.WinApp.Views.Orders;

public interface IOrderLineItemEditView : MVP.IView
{
    event EventHandler? SaveRequested;
    event EventHandler? CancelRequested;
    CleanCut.Application.DTOs.OrderLineItemInfo GetLineItemData();
    void SetLineItemData(CleanCut.Application.DTOs.OrderLineItemInfo lineItem);
    Dictionary<string, string> ValidateForm();
    void ClearForm();
}
