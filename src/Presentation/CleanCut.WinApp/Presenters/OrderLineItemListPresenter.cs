using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CleanCut.Application.DTOs;
using CleanCut.WinApp.Views.Orders;
using CleanCut.WinApp.MVP;

namespace CleanCut.WinApp.Presenters;

public class OrderLineItemListPresenter : BasePresenter<IOrderLineItemListView>
{
    private readonly Services.Factories.IViewFactory<IOrderLineItemEditView> _editViewFactory;
    private List<OrderLineItemInfo> _lineItems = new();
    public event Action? LineItemsChanged;

    public OrderLineItemListPresenter(IOrderLineItemListView view, Services.Factories.IViewFactory<IOrderLineItemEditView> editViewFactory)
        : base(view)
    {
        _editViewFactory = editViewFactory;
        View.AddLineItemRequested += (s, e) => AddLineItem();
        View.EditLineItemRequested += (s, id) => EditLineItem(id);
        View.DeleteLineItemRequested += (s, id) => DeleteLineItem(id);
        View.RefreshRequested += (s, e) => Refresh();
    }

    public void Load(IEnumerable<OrderLineItemInfo> lineItems)
    {
        _lineItems = new List<OrderLineItemInfo>(lineItems);
        View.DisplayLineItems(_lineItems);
    }

    private void AddLineItem()
    {
        var editView = _editViewFactory.Create();
        editView.ClearForm();
        if (editView is Form form)
        {
            var result = form.ShowDialog();
            if (result == DialogResult.OK)
            {
                var newItem = editView.GetLineItemData();
                _lineItems.Add(newItem);
                View.DisplayLineItems(_lineItems);
                LineItemsChanged?.Invoke();
            }
        }
    }

    private void EditLineItem(Guid id)
    {
        var item = _lineItems.Find(x => x.Id == id);
        if (item == null) return;
        var editView = _editViewFactory.Create();
        editView.SetLineItemData(item);
        if (editView is Form form)
        {
            var result = form.ShowDialog();
            if (result == DialogResult.OK)
            {
                var updated = editView.GetLineItemData();
                var idx = _lineItems.FindIndex(x => x.Id == id);
                if (idx >= 0) _lineItems[idx] = updated;
                View.DisplayLineItems(_lineItems);
                LineItemsChanged?.Invoke();
            }
        }
    }

    private void DeleteLineItem(Guid id)
    {
        _lineItems.RemoveAll(x => x.Id == id);
        View.DisplayLineItems(_lineItems);
        LineItemsChanged?.Invoke();
    }

    private void Refresh()
    {
        View.DisplayLineItems(_lineItems);
    }
}
