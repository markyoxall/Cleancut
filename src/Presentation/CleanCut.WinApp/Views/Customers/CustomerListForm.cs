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

    private ListView _listView = null!;
    private Button _addButton = null!;
    private Button _editButton = null!;
    private Button _deleteButton = null!;
    private Button _refreshButton = null!;

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

    private void InitializeComponent()
    {
        _listView = new ListView();
        _addButton = new Button();
        _editButton = new Button();
        _deleteButton = new Button();
        _refreshButton = new Button();
        
        SuspendLayout();
        
        // 
        // _listView
        // 
        _listView.View = View.Details;
        _listView.FullRowSelect = true;
        _listView.GridLines = true;
        _listView.MultiSelect = false;
        _listView.Location = new Point(12, 12);
        _listView.Size = new Size(776, 350);
        _listView.Columns.Add("First Name", 150);
        _listView.Columns.Add("Last Name", 150);
        _listView.Columns.Add("Email", 250);
        _listView.Columns.Add("Status", 100);
        _listView.Columns.Add("Created", 100);
        
        // 
        // _addButton
        // 
        _addButton.Location = new Point(12, 380);
        _addButton.Size = new Size(100, 30);
        _addButton.Text = "Add Customer";
        _addButton.UseVisualStyleBackColor = true;
        
        // 
        // _editButton
        // 
        _editButton.Location = new Point(120, 380);
        _editButton.Size = new Size(100, 30);
        _editButton.Text = "Edit Customer";
        _editButton.UseVisualStyleBackColor = true;
        _editButton.Enabled = false;
        
        // 
        // _deleteButton
        // 
        _deleteButton.Location = new Point(228, 380);
        _deleteButton.Size = new Size(100, 30);
        _deleteButton.Text = "Delete Customer";
        _deleteButton.UseVisualStyleBackColor = true;
        _deleteButton.Enabled = false;
        
        // 
        // _refreshButton
        // 
        _refreshButton.Location = new Point(688, 380);
        _refreshButton.Size = new Size(100, 30);
        _refreshButton.Text = "Refresh";
        _refreshButton.UseVisualStyleBackColor = true;
        
        // 
        // CustomerListForm
        // 
        ClientSize = new Size(800, 450);
        Controls.Add(_listView);
        Controls.Add(_addButton);
        Controls.Add(_editButton);
        Controls.Add(_deleteButton);
        Controls.Add(_refreshButton);
        Text = "Customer Management - CleanCut";
        
        ResumeLayout(false);
    }
}