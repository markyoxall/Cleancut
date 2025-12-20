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
    public event EventHandler<Guid>? ViewProductsByCustomerRequested;

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
            if (_userFilterComboBox.SelectedItem is CustomerInfo selectedUser)
                ViewProductsByCustomerRequested?.Invoke(this, selectedUser.Id);
        };
        _gridView.FocusedRowChanged += (s, e) => UpdateButtonStates();
    }

    public void DisplayProducts(IEnumerable<ProductInfo> products)
    {
        if (InvokeRequired)
        {
            Invoke(() => DisplayProducts(products));
            return;
        }

        _gridControl.DataSource = products.ToList();
        UpdateButtonStates();
    }

    public void ClearProducts()
    {
        if (InvokeRequired)
        {
            Invoke(ClearProducts);
            return;
        }
        
        _gridControl.DataSource = null;
        UpdateButtonStates();
    }

    public Guid? GetSelectedProductId()
    {
        if (_gridView.FocusedRowHandle < 0)
            return null;
        var row = _gridView.GetRow(_gridView.FocusedRowHandle) as ProductInfo;
        return row?.Id;
    }

    public void SetAvailableUsers(IEnumerable<CustomerInfo> users)
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

    /// <summary>
    /// Set available customers for filtering (alias for SetAvailableUsers to match updated terminology)
    /// </summary>
    public void SetAvailableCustomers(IEnumerable<CustomerInfo> customers)
    {
        SetAvailableUsers(customers);
    }

    private void UpdateButtonStates()
    {
        var hasSelection = _gridView.FocusedRowHandle >= 0 && _gridView.GetRow(_gridView.FocusedRowHandle) is ProductInfo;
        _editButton.Enabled = hasSelection;
        _deleteButton.Enabled = hasSelection;
        var hasUserSelected = _userFilterComboBox.SelectedItem is CustomerInfo;
        _filterButton.Enabled = hasUserSelected;
    }


}
