using System.Collections.Generic;
using CleanCut.Application.DTOs;

namespace CleanCut.WinApp.Views.Orders;

public class OrderLineItemListForm : DevExpress.XtraEditors.XtraForm, IOrderLineItemListView
{
    public event EventHandler? AddLineItemRequested;
    public event EventHandler<Guid>? EditLineItemRequested;
    public event EventHandler<Guid>? DeleteLineItemRequested;
    public event EventHandler? RefreshRequested;

    private DevExpress.XtraGrid.GridControl _gridControl;
    private DevExpress.XtraGrid.Views.Grid.GridView _gridView;
    private DevExpress.XtraEditors.SimpleButton _addButton;
    private DevExpress.XtraEditors.SimpleButton _editButton;
    private DevExpress.XtraEditors.SimpleButton _deleteButton;
    private DevExpress.XtraEditors.SimpleButton _refreshButton;

    public OrderLineItemListForm()
    {
        _gridControl = new DevExpress.XtraGrid.GridControl();
        _gridView = new DevExpress.XtraGrid.Views.Grid.GridView();
        _addButton = new DevExpress.XtraEditors.SimpleButton();
        _editButton = new DevExpress.XtraEditors.SimpleButton();
        _deleteButton = new DevExpress.XtraEditors.SimpleButton();
        _refreshButton = new DevExpress.XtraEditors.SimpleButton();
        InitializeComponent();
        SetupEventHandlers();
    }

    private void InitializeComponent()
    {
        SuspendLayout();
        _gridControl.Location = new System.Drawing.Point(12, 12);
        _gridControl.Size = new System.Drawing.Size(700, 300);
        _gridControl.MainView = _gridView;
        _gridControl.ViewCollection.Add(_gridView);
        _gridView.OptionsBehavior.Editable = false;
        _gridView.OptionsSelection.EnableAppearanceFocusedCell = false;
        _gridView.OptionsView.ShowGroupPanel = false;
        _gridView.OptionsView.ShowIndicator = false;
        _gridView.Columns.AddVisible("ProductName", "Product");
        _gridView.Columns.AddVisible("UnitPrice", "Unit Price");
        _gridView.Columns.AddVisible("Quantity", "Quantity");
        _gridView.Columns.AddVisible("LineTotal", "Line Total");
        _gridView.Columns.AddVisible("CreatedAt", "Created");
        _gridView.Columns.AddVisible("UpdatedAt", "Updated");
        _addButton.Text = "Add";
        _addButton.Location = new System.Drawing.Point(12, 320);
        _editButton.Text = "Edit";
        _editButton.Location = new System.Drawing.Point(100, 320);
        _deleteButton.Text = "Delete";
        _deleteButton.Location = new System.Drawing.Point(188, 320);
        _refreshButton.Text = "Refresh";
        _refreshButton.Location = new System.Drawing.Point(612, 320);
        Controls.Add(_gridControl);
        Controls.Add(_addButton);
        Controls.Add(_editButton);
        Controls.Add(_deleteButton);
        Controls.Add(_refreshButton);
        ClientSize = new System.Drawing.Size(730, 370);
        Text = "Order Line Items";
        ResumeLayout(false);
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
