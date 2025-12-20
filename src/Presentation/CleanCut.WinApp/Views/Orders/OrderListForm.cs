using System.Collections.Generic;
using CleanCut.Application.DTOs;

namespace CleanCut.WinApp.Views.Orders;

public partial class OrderListForm : DevExpress.XtraEditors.XtraForm, IOrderListView
{
    public event EventHandler? AddOrderRequested;
    public event EventHandler<Guid>? EditOrderRequested;
    public event EventHandler<Guid>? DeleteOrderRequested;
    public event EventHandler? RefreshRequested;
    public event EventHandler<Guid>? ViewOrdersByCustomerRequested;
    public event EventHandler<Guid>? ViewOrderLineItemsRequested;

    public OrderListForm()
    {
        InitializeComponent();
        SetupEventHandlers();
    }

    private void SetupEventHandlers()
    {
        _addButton.Click += (s, e) => AddOrderRequested?.Invoke(this, EventArgs.Empty);
        _editButton.Click += (s, e) => { var id = GetSelectedOrderId(); if (id.HasValue) EditOrderRequested?.Invoke(this, id.Value); };
        _deleteButton.Click += (s, e) => { var id = GetSelectedOrderId(); if (id.HasValue) DeleteOrderRequested?.Invoke(this, id.Value); };
        _refreshButton.Click += (s, e) => RefreshRequested?.Invoke(this, EventArgs.Empty);
        _viewLineItemsButton.Click += (s, e) => { var id = GetSelectedOrderId(); if (id.HasValue) ViewOrderLineItemsRequested?.Invoke(this, id.Value); };
        _gridView.FocusedRowChanged += (s, e) => UpdateButtonStates();
    }

    public void DisplayOrders(IEnumerable<OrderInfo> orders)
    {
        _gridControl.DataSource = orders is List<OrderInfo> list ? list : new List<OrderInfo>(orders);
        UpdateButtonStates();
    }

    public void ClearOrders()
    {
        _gridControl.DataSource = null;
        UpdateButtonStates();
    }

    public Guid? GetSelectedOrderId()
    {
        if (_gridView.FocusedRowHandle < 0)
            return null;
        var row = _gridView.GetRow(_gridView.FocusedRowHandle) as OrderInfo;
        return row?.Id;
    }

    public void SetAvailableCustomers(IEnumerable<CleanCut.Application.DTOs.CustomerInfo> customers) { /* implement if needed */ }
    private void UpdateButtonStates()
    {
        var hasSelection = _gridView.FocusedRowHandle >= 0 && _gridView.GetRow(_gridView.FocusedRowHandle) is OrderInfo;
        _editButton.Enabled = hasSelection;
        _deleteButton.Enabled = hasSelection;
        _viewLineItemsButton.Enabled = hasSelection;
    }

    public void ShowError(string message) => DevExpress.XtraEditors.XtraMessageBox.Show(this, message, "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
    public void ShowInfo(string message) => DevExpress.XtraEditors.XtraMessageBox.Show(this, message, "Info", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
    public void ShowSuccess(string message) => DevExpress.XtraEditors.XtraMessageBox.Show(this, message, "Success", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
    public bool ShowConfirmation(string message) => DevExpress.XtraEditors.XtraMessageBox.Show(this, message, "Confirm", System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes;
    public void SetLoading(bool isLoading) { /* Optionally implement a loading indicator */ }
}
