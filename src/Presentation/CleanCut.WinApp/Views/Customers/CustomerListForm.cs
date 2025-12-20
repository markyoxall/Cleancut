using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CleanCut.Application.DTOs;
using CleanCut.WinApp.MVP;

namespace CleanCut.WinApp.Views.Customers;

/// <summary>
/// Customer List Form implementing MVP pattern
/// </summary>
public partial class CustomerListForm : BaseForm, ICustomerListView
{
    public event EventHandler? AddCustomerRequested;
    public event EventHandler<Guid>? EditCustomerRequested;
    public event EventHandler<Guid>? DeleteCustomerRequested;
    public event EventHandler? RefreshRequested;

    public CustomerListForm()
    {
        InitializeComponent();
        SetupEventHandlers();
    }

    private void SetupEventHandlers()
    {
        _addButton.Click += (s, e) => AddCustomerRequested?.Invoke(this, EventArgs.Empty);
        _editButton.Click += (s, e) => {
            var userId = GetSelectedCustomerId();
            if (userId.HasValue)
                EditCustomerRequested?.Invoke(this, userId.Value);
        };
        _deleteButton.Click += (s, e) => {
            var userId = GetSelectedCustomerId();
            if (userId.HasValue)
                DeleteCustomerRequested?.Invoke(this, userId.Value);
        };
        _refreshButton.Click += (s, e) => RefreshRequested?.Invoke(this, EventArgs.Empty);
        _gridView.FocusedRowChanged += (s, e) => UpdateButtonStates();
    }

    public void DisplayCustomers(IEnumerable<CustomerInfo> users)
    {
        if (InvokeRequired)
        {
            Invoke(() => DisplayCustomers(users));
            return;
        }
        _gridControl.DataSource = users is List<CustomerInfo> list ? list : new List<CustomerInfo>(users);
        UpdateButtonStates();
    }

    public void ClearCustomers()
    {
        if (InvokeRequired)
        {
            Invoke(ClearCustomers);
            return;
        }
        _gridControl.DataSource = null;
        UpdateButtonStates();
    }

    public Guid? GetSelectedCustomerId()
    {
        if (_gridView.FocusedRowHandle < 0)
            return null;
        var row = _gridView.GetRow(_gridView.FocusedRowHandle) as CustomerInfo;
        return row?.Id;
    }

    private void UpdateButtonStates()
    {
        var hasSelection = _gridView.FocusedRowHandle >= 0 && _gridView.GetRow(_gridView.FocusedRowHandle) is CustomerInfo;
        _editButton.Enabled = hasSelection;
        _deleteButton.Enabled = hasSelection;
    }
}
