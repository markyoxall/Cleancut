using System.Collections.Generic;
using CleanCut.Application.DTOs;

namespace CleanCut.WinApp.Views.Orders;

public class OrderListForm : DevExpress.XtraEditors.XtraForm, IOrderListView
{
    public event EventHandler? AddOrderRequested;
    public event EventHandler<Guid>? EditOrderRequested;
    public event EventHandler<Guid>? DeleteOrderRequested;
    public event EventHandler? RefreshRequested;
    public event EventHandler<Guid>? ViewOrdersByCustomerRequested;
    public event EventHandler<Guid>? ViewOrderLineItemsRequested;

    private DevExpress.XtraGrid.GridControl _gridControl;
    private DevExpress.XtraGrid.Views.Grid.GridView _gridView;
    private DevExpress.XtraEditors.SimpleButton _addButton;
    private DevExpress.XtraEditors.SimpleButton _editButton;
    private DevExpress.XtraEditors.SimpleButton _deleteButton;
    private DevExpress.XtraEditors.SimpleButton _refreshButton;
    private DevExpress.XtraEditors.SimpleButton _viewLineItemsButton;

    public OrderListForm()
    {
        _gridControl = new DevExpress.XtraGrid.GridControl();
        _gridView = new DevExpress.XtraGrid.Views.Grid.GridView();
        _addButton = new DevExpress.XtraEditors.SimpleButton();
        _editButton = new DevExpress.XtraEditors.SimpleButton();
        _deleteButton = new DevExpress.XtraEditors.SimpleButton();
        _refreshButton = new DevExpress.XtraEditors.SimpleButton();
        _viewLineItemsButton = new DevExpress.XtraEditors.SimpleButton();
        InitializeComponent();
        SetupEventHandlers();
    }

    private void InitializeComponent()
    {
        SuspendLayout();
        _gridControl.Location = new System.Drawing.Point(12, 12);
        _gridControl.Size = new System.Drawing.Size(900, 400);
        _gridControl.MainView = _gridView;
        _gridControl.ViewCollection.Add(_gridView);
        _gridView.OptionsBehavior.Editable = false;
        _gridView.OptionsSelection.EnableAppearanceFocusedCell = false;
        _gridView.OptionsView.ShowGroupPanel = false;
        _gridView.OptionsView.ShowIndicator = false;
        _gridView.Columns.AddVisible("OrderNumber", "Order #");
        _gridView.Columns.AddVisible("OrderDate", "Date");
        _gridView.Columns.AddVisible("CustomerName", "Customer");
        _gridView.Columns.AddVisible("StatusName", "Status");
        _gridView.Columns.AddVisible("TotalAmount", "Total");
        _gridView.Columns.AddVisible("TotalItemCount", "Items");
        _gridView.Columns.AddVisible("CreatedAt", "Created");
        _gridView.Columns.AddVisible("UpdatedAt", "Updated");
        _addButton.Text = "Add";
        _addButton.Location = new System.Drawing.Point(12, 420);
        _editButton.Text = "Edit";
        _editButton.Location = new System.Drawing.Point(100, 420);
        _deleteButton.Text = "Delete";
        _deleteButton.Location = new System.Drawing.Point(188, 420);
        _refreshButton.Text = "Refresh";
        _refreshButton.Location = new System.Drawing.Point(800, 420);
        _viewLineItemsButton.Text = "View Line Items";
        _viewLineItemsButton.Location = new System.Drawing.Point(276, 420);
        Controls.Add(_gridControl);
        Controls.Add(_addButton);
        Controls.Add(_editButton);
        Controls.Add(_deleteButton);
        Controls.Add(_refreshButton);
        Controls.Add(_viewLineItemsButton);
        ClientSize = new System.Drawing.Size(930, 470);
        Text = "Order Management";
        ResumeLayout(false);
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
