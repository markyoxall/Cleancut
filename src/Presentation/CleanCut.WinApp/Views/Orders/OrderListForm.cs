using System;
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
        ConfigureGrid();
    }
    private void ConfigureGrid()
    {
        var view = gridView1;
        view.Columns.Clear();

        // Dynamically add columns based on OrderListGridItem properties
        var properties = typeof(OrderListGridItem).GetProperties();
        foreach (var prop in properties)
        {
            var column = view.Columns.AddVisible(prop.Name, SplitCamelCase(prop.Name));
            column.Visible = true;
            if (prop.PropertyType == typeof(DateTime) || prop.PropertyType == typeof(DateTime?))
            {
                column.DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
                column.DisplayFormat.FormatString = "d";
            }
            else if (prop.PropertyType == typeof(decimal) || prop.PropertyType == typeof(double) || prop.PropertyType == typeof(float))
            {
                column.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
                column.DisplayFormat.FormatString = "c";
            }
            column.OptionsColumn.AllowEdit = false;
        }

        view.OptionsBehavior.Editable = false;
        view.OptionsView.ShowGroupPanel = false;
        view.OptionsSelection.EnableAppearanceFocusedCell = false;
    }

    private static string SplitCamelCase(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        var result = System.Text.RegularExpressions.Regex.Replace(input, "([a-z])([A-Z])", "$1 $2");
        return result;
    }

    private void SetupEventHandlers()
    {
        _addButton.Click += (s, e) => AddOrderRequested?.Invoke(this, EventArgs.Empty);
        _editButton.Click += (s, e) => { var id = GetSelectedOrderId(); if (id.HasValue) EditOrderRequested?.Invoke(this, id.Value); };
        _deleteButton.Click += (s, e) => { var id = GetSelectedOrderId(); if (id.HasValue) DeleteOrderRequested?.Invoke(this, id.Value); };
        _refreshButton.Click += (s, e) => RefreshRequested?.Invoke(this, EventArgs.Empty);
        _viewLineItemsButton.Click += (s, e) => { var id = GetSelectedOrderId(); if (id.HasValue) ViewOrderLineItemsRequested?.Invoke(this, id.Value); };
        gridView1.FocusedRowChanged += (s, e) => UpdateButtonStates();
    }

    public void DisplayOrders(IEnumerable<OrderListGridItem> orders)
    {
        _gridControl.DataSource = orders is List<OrderListGridItem> list ? list : new List<OrderListGridItem>(orders);
        gridView1.RefreshData();
        UpdateButtonStates();
    }

    public void ClearOrders()
    {
        _gridControl.DataSource = null;
        UpdateButtonStates();
    }

    public Guid? GetSelectedOrderId()
    {
        var view = gridView1;
        if (view.FocusedRowHandle < 0)
            return null;
        var row = view.GetRow(view.FocusedRowHandle) as OrderListGridItem;
        return row?.Id;
    }

    public void SetAvailableCustomers(IEnumerable<CleanCut.Application.DTOs.CustomerInfo> customers) { /* implement if needed */ }
    private void UpdateButtonStates()
    {
        var view = gridView1;
        var hasSelection = view.FocusedRowHandle >= 0 && view.GetRow(view.FocusedRowHandle) is OrderListGridItem;
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
