using CleanCut.Application.DTOs;
using CleanCut.WinApp.MVP;

namespace CleanCut.WinApp.Views.Products;

/// <summary>
/// Product List Form implementing MVP pattern
/// </summary>
public partial class ProductListForm : BaseForm, IProductListView
{
    public event EventHandler? AddProductRequested;
    public event EventHandler<Guid>? EditProductRequested;
    public event EventHandler<Guid>? DeleteProductRequested;
    public event EventHandler? RefreshRequested;
    public event EventHandler<Guid>? ViewProductsByUserRequested;

    private ListView _listView = null!;
    private ComboBox _userFilterComboBox = null!;
    private Button _addButton = null!;
    private Button _editButton = null!;
    private Button _deleteButton = null!;
    private Button _refreshButton = null!;
    private Button _filterButton = null!;
    private Label _userFilterLabel = null!;

    public ProductListForm()
    {
        InitializeComponent();
        SetupEventHandlers();
    }

    private void SetupEventHandlers()
    {
        _addButton.Click += (s, e) => AddProductRequested?.Invoke(this, EventArgs.Empty);
        _editButton.Click += (s, e) => {
            var productId = GetSelectedProductId();
            if (productId.HasValue)
                EditProductRequested?.Invoke(this, productId.Value);
        };
        _deleteButton.Click += (s, e) => {
            var productId = GetSelectedProductId();
            if (productId.HasValue)
                DeleteProductRequested?.Invoke(this, productId.Value);
        };
        _refreshButton.Click += (s, e) => RefreshRequested?.Invoke(this, EventArgs.Empty);
        _filterButton.Click += (s, e) => {
            if (_userFilterComboBox.SelectedItem is UserDto selectedUser)
                ViewProductsByUserRequested?.Invoke(this, selectedUser.Id);
        };
        
        _listView.SelectedIndexChanged += (s, e) => UpdateButtonStates();
    }

    public void DisplayProducts(IEnumerable<ProductDto> products)
    {
        if (InvokeRequired)
        {
            Invoke(() => DisplayProducts(products));
            return;
        }

        _listView.Items.Clear();
        
        foreach (var product in products)
        {
            var item = new ListViewItem(product.Name)
            {
                Tag = product.Id
            };
            item.SubItems.Add(product.Description);
            item.SubItems.Add(product.Price.ToString("C"));
            item.SubItems.Add(product.IsAvailable ? "Available" : "Unavailable");
            item.SubItems.Add(product.User?.GetFullName() ?? "Unknown User");
            item.SubItems.Add(product.CreatedAt.ToString("yyyy-MM-dd"));
            
            _listView.Items.Add(item);
        }
        
        UpdateButtonStates();
    }

    public void ClearProducts()
    {
        if (InvokeRequired)
        {
            Invoke(ClearProducts);
            return;
        }
        
        _listView.Items.Clear();
        UpdateButtonStates();
    }

    public Guid? GetSelectedProductId()
    {
        if (_listView.SelectedItems.Count == 0)
            return null;
            
        var tag = _listView.SelectedItems[0].Tag;
        return tag as Guid?;
    }

    public void SetAvailableUsers(IEnumerable<UserDto> users)
    {
        if (InvokeRequired)
        {
            Invoke(() => SetAvailableUsers(users));
            return;
        }

        _userFilterComboBox.Items.Clear();
        _userFilterComboBox.Items.Add("All Users");
        
        foreach (var user in users)
        {
            _userFilterComboBox.Items.Add(user);
        }
        
        _userFilterComboBox.SelectedIndex = 0;
    }

    private void UpdateButtonStates()
    {
        var hasSelection = _listView.SelectedItems.Count > 0;
        _editButton.Enabled = hasSelection;
        _deleteButton.Enabled = hasSelection;
        
        var hasUserSelected = _userFilterComboBox.SelectedItem is UserDto;
        _filterButton.Enabled = hasUserSelected;
    }

    private void InitializeComponent()
    {
        _listView = new ListView();
        _userFilterLabel = new Label();
        _userFilterComboBox = new ComboBox();
        _filterButton = new Button();
        _addButton = new Button();
        _editButton = new Button();
        _deleteButton = new Button();
        _refreshButton = new Button();
        
        SuspendLayout();
        
        // 
        // _userFilterLabel
        // 
        _userFilterLabel.AutoSize = true;
        _userFilterLabel.Location = new Point(12, 15);
        _userFilterLabel.Size = new Size(60, 15);
        _userFilterLabel.Text = "Filter by User:";
        
        // 
        // _userFilterComboBox
        // 
        _userFilterComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        _userFilterComboBox.Location = new Point(80, 12);
        _userFilterComboBox.Size = new Size(200, 23);
        _userFilterComboBox.DisplayMember = "FullName";
        _userFilterComboBox.SelectedIndexChanged += (s, e) => UpdateButtonStates();
        
        // 
        // _filterButton
        // 
        _filterButton.Location = new Point(290, 12);
        _filterButton.Size = new Size(60, 23);
        _filterButton.Text = "Filter";
        _filterButton.UseVisualStyleBackColor = true;
        _filterButton.Enabled = false;
        
        // 
        // _listView
        // 
        _listView.View = View.Details;
        _listView.FullRowSelect = true;
        _listView.GridLines = true;
        _listView.MultiSelect = false;
        _listView.Location = new Point(12, 45);
        _listView.Size = new Size(776, 320);
        _listView.Columns.Add("Name", 150);
        _listView.Columns.Add("Description", 200);
        _listView.Columns.Add("Price", 100);
        _listView.Columns.Add("Status", 100);
        _listView.Columns.Add("Owner", 150);
        _listView.Columns.Add("Created", 100);
        
        // 
        // _addButton
        // 
        _addButton.Location = new Point(12, 380);
        _addButton.Size = new Size(100, 30);
        _addButton.Text = "Add Product";
        _addButton.UseVisualStyleBackColor = true;
        
        // 
        // _editButton
        // 
        _editButton.Location = new Point(120, 380);
        _editButton.Size = new Size(100, 30);
        _editButton.Text = "Edit Product";
        _editButton.UseVisualStyleBackColor = true;
        _editButton.Enabled = false;
        
        // 
        // _deleteButton
        // 
        _deleteButton.Location = new Point(228, 380);
        _deleteButton.Size = new Size(100, 30);
        _deleteButton.Text = "Delete Product";
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
        // ProductListForm
        // 
        ClientSize = new Size(800, 450);
        Controls.Add(_userFilterLabel);
        Controls.Add(_userFilterComboBox);
        Controls.Add(_filterButton);
        Controls.Add(_listView);
        Controls.Add(_addButton);
        Controls.Add(_editButton);
        Controls.Add(_deleteButton);
        Controls.Add(_refreshButton);
        Text = "Product Management - CleanCut";
        
        ResumeLayout(false);
        PerformLayout();
    }
}