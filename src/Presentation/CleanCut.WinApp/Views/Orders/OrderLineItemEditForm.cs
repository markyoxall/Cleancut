using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CleanCut.Application.DTOs;

namespace CleanCut.WinApp.Views.Orders;

public class OrderLineItemEditForm : DevExpress.XtraEditors.XtraForm, IOrderLineItemEditView
{
    public event EventHandler? SaveRequested;
    public event EventHandler? CancelRequested;

    private DevExpress.XtraEditors.TextEdit _productName;
    private DevExpress.XtraEditors.TextEdit _unitPrice;
    private DevExpress.XtraEditors.SpinEdit _quantity;
    private DevExpress.XtraEditors.SimpleButton _saveButton;
    private DevExpress.XtraEditors.SimpleButton _cancelButton;

    public OrderLineItemEditForm()
    {
        _productName = new DevExpress.XtraEditors.TextEdit();
        _unitPrice = new DevExpress.XtraEditors.TextEdit();
        _quantity = new DevExpress.XtraEditors.SpinEdit();
        _saveButton = new DevExpress.XtraEditors.SimpleButton();
        _cancelButton = new DevExpress.XtraEditors.SimpleButton();
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        SuspendLayout();
        _productName.Location = new System.Drawing.Point(12, 12);
        _productName.Size = new System.Drawing.Size(400, 22);
        _unitPrice.Location = new System.Drawing.Point(12, 50);
        _unitPrice.Size = new System.Drawing.Size(200, 22);
        _quantity.Location = new System.Drawing.Point(12, 88);
        _quantity.Size = new System.Drawing.Size(100, 22);
        _saveButton.Text = "Save";
        _saveButton.Location = new System.Drawing.Point(12, 130);
        _cancelButton.Text = "Cancel";
        _cancelButton.Location = new System.Drawing.Point(100, 130);
        _saveButton.Click += (s, e) => { SaveRequested?.Invoke(this, EventArgs.Empty); DialogResult = DialogResult.OK; Close(); };
        _cancelButton.Click += (s, e) => { CancelRequested?.Invoke(this, EventArgs.Empty); DialogResult = DialogResult.Cancel; Close(); };
        Controls.Add(_productName);
        Controls.Add(_unitPrice);
        Controls.Add(_quantity);
        Controls.Add(_saveButton);
        Controls.Add(_cancelButton);
        Text = "Edit Line Item";
        ClientSize = new System.Drawing.Size(430, 180);
        ResumeLayout(false);
    }

    public OrderLineItemInfo GetLineItemData()
    {
        return new OrderLineItemInfo
        {
            Id = Guid.NewGuid(),
            ProductId = Guid.Empty,
            ProductName = _productName.Text,
            UnitPrice = decimal.TryParse(_unitPrice.Text, out var p) ? p : 0m,
            Quantity = (int)_quantity.Value,
            LineTotal = decimal.TryParse(_unitPrice.Text, out var pt) ? pt * (int)_quantity.Value : 0m,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void SetLineItemData(OrderLineItemInfo lineItem)
    {
        _productName.Text = lineItem.ProductName;
        _unitPrice.Text = lineItem.UnitPrice.ToString();
        _quantity.Value = lineItem.Quantity;
    }

    public Dictionary<string, string> ValidateForm()
    {
        var errors = new Dictionary<string, string>();
        if (string.IsNullOrWhiteSpace(_productName.Text)) errors[nameof(_productName)] = "Product is required";
        if (!decimal.TryParse(_unitPrice.Text, out var p) || p < 0) errors[nameof(_unitPrice)] = "Unit price must be a positive number";
        if ((int)_quantity.Value <= 0) errors[nameof(_quantity)] = "Quantity must be positive";
        return errors;
    }

    public void ClearForm()
    {
        _productName.Text = string.Empty;
        _unitPrice.Text = string.Empty;
        _quantity.Value = 1;
    }

    public void ShowError(string message) => DevExpress.XtraEditors.XtraMessageBox.Show(this, message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
    public void ShowInfo(string message) => DevExpress.XtraEditors.XtraMessageBox.Show(this, message, "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
    public void ShowSuccess(string message) => DevExpress.XtraEditors.XtraMessageBox.Show(this, message, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
    public bool ShowConfirmation(string message) => DevExpress.XtraEditors.XtraMessageBox.Show(this, message, "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
    public void SetLoading(bool isLoading) { Enabled = !isLoading; Cursor = isLoading ? Cursors.WaitCursor : Cursors.Default; }
}
