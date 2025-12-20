using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CleanCut.Application.DTOs;

namespace CleanCut.WinApp.Views.Orders;

public partial class OrderLineItemEditForm : DevExpress.XtraEditors.XtraForm, IOrderLineItemEditView
{
    public event EventHandler? SaveRequested;
    public event EventHandler? CancelRequested;

    public OrderLineItemEditForm()
    {
        InitializeComponent();
        _saveButton.Click += (s, e) => { SaveRequested?.Invoke(this, EventArgs.Empty); DialogResult = DialogResult.OK; Close(); };
        _cancelButton.Click += (s, e) => { CancelRequested?.Invoke(this, EventArgs.Empty); DialogResult = DialogResult.Cancel; Close(); };
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
