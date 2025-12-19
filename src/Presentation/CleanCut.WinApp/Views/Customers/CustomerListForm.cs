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
        
        _listView.SelectedIndexChanged += (s, e) => UpdateButtonStates();
    }

    public void DisplayCustomers(IEnumerable<CustomerInfo> users)
    {
        if (InvokeRequired)
        {
            Invoke(() => DisplayCustomers(users));
            return;
        }

        _listView.Items.Clear();
        
        foreach (var user in users)
        {
            var item = new ListViewItem(user.FirstName)
            {
                Tag = user.Id
            };
            item.SubItems.Add(user.LastName);
            item.SubItems.Add(user.Email);
            item.SubItems.Add(user.IsActive ? "Active" : "Inactive");
            item.SubItems.Add(user.CreatedAt.ToString("yyyy-MM-dd"));
            
            _listView.Items.Add(item);
        }
        
        UpdateButtonStates();
    }

    public void ClearCustomers()
    {
        if (InvokeRequired)
        {
            Invoke(ClearCustomers);
            return;
        }
        
        _listView.Items.Clear();
        UpdateButtonStates();
    }

    public Guid? GetSelectedCustomerId()
    {
        if (_listView.SelectedItems.Count == 0)
            return null;
            
        var tag = _listView.SelectedItems[0].Tag;
        return tag as Guid?;
    }

    private void UpdateButtonStates()
    {
        var hasSelection = _listView.SelectedItems.Count > 0;
        _editButton.Enabled = hasSelection;
        _deleteButton.Enabled = hasSelection;
    }

 
}
