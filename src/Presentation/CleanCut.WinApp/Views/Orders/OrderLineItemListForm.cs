using System.Collections.Generic;
using CleanCut.Application.DTOs;

namespace CleanCut.WinApp.Views.Orders;

public partial class OrderLineItemListForm : DevExpress.XtraEditors.XtraForm, IOrderLineItemListView
{
    public event EventHandler? AddLineItemRequested;
    public event EventHandler<Guid>? EditLineItemRequested;
    public event EventHandler<Guid>? DeleteLineItemRequested;
    public event EventHandler? RefreshRequested;

    public OrderLineItemListForm()
    {
        InitializeComponent();
        SetupEventHandlers();
    }

    private void SetupEventHandlers()
    {
        _addButton.Click += (s, e) => AddLineItemRequested?.Invoke(this, EventArgs.Empty);
        _editButton.Click += (s, e) => { var id = GetSelectedLineItemId(); if (id.HasValue) EditLineItemRequested?.Invoke(this, id.Value); };
        _deleteButton.Click += (s, e) => { var id = GetSelectedLineItemId(); if (id.HasValue) DeleteLineItemRequested?.Invoke(this, id.Value); };
        _refreshButton.Click += (s, e) => RefreshRequested?.Invoke(this, EventArgs.Empty);
        _gridView.FocusedRowChanged += (s, e) => UpdateButtonStates();
    }

    public void DisplayLineItems(IEnumerable<OrderLineItemInfo> lineItems)
    {
        _gridControl.DataSource = lineItems is List<OrderLineItemInfo> list ? list : new List<OrderLineItemInfo>(lineItems);
        UpdateButtonStates();
    }

    public void ClearLineItems()
    {
        _gridControl.DataSource = null;
        UpdateButtonStates();
    }

    public Guid? GetSelectedLineItemId()
    {
        if (_gridView.FocusedRowHandle < 0)
            return null;
        var row = _gridView.GetRow(_gridView.FocusedRowHandle) as OrderLineItemInfo;
        return row?.Id;
    }

    private void UpdateButtonStates()
    {
        var hasSelection = _gridView.FocusedRowHandle >= 0 && _gridView.GetRow(_gridView.FocusedRowHandle) is OrderLineItemInfo;
        _editButton.Enabled = hasSelection;
        _deleteButton.Enabled = hasSelection;
    }

    public void ShowError(string message)
    {
        throw new NotImplementedException();
    }

    public void ShowInfo(string message)
    {
        throw new NotImplementedException();
    }

    public void ShowSuccess(string message)
    {
        throw new NotImplementedException();
    }

    public bool ShowConfirmation(string message)
    {
        throw new NotImplementedException();
    }

    public void SetLoading(bool isLoading)
    {
        throw new NotImplementedException();
    }
}
