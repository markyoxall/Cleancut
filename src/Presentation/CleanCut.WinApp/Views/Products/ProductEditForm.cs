using CleanCut.Application.DTOs;
using CleanCut.WinApp.MVP;

namespace CleanCut.WinApp.Views.Products;

/// <summary>
/// Product Edit Form implementing MVP pattern
/// </summary>
public partial class ProductEditForm : BaseForm, IProductEditView
{
    public event EventHandler? SaveRequested;
    public event EventHandler? CancelRequested;

      public ProductEditForm()
    {
        InitializeComponent();
        SetupEventHandlers();
    }

    private void SetupEventHandlers()
    {
        _saveButton.Click += (s, e) => SaveRequested?.Invoke(this, EventArgs.Empty);
        _cancelButton.Click += (s, e) => CancelRequested?.Invoke(this, EventArgs.Empty);
    }

    public ProductEditViewModel GetProductData()
    {
        var selectedUser = _userComboBox.SelectedItem as CustomerInfo;
        
        return new ProductEditViewModel
        {
            Name = _nameTextBox.Text.Trim(),
            Description = _descriptionTextBox.Text.Trim(),
            Price = _priceNumericUpDown.Value,
            IsAvailable = _isAvailableCheckBox.Checked,
            UserId = selectedUser?.Id ?? Guid.Empty
        };
    }

    public void SetProductData(ProductEditViewModel product)
    {
        if (InvokeRequired)
        {
            Invoke(() => SetProductData(product));
            return;
        }

        _nameTextBox.Text = product.Name;
        _descriptionTextBox.Text = product.Description;
        _priceNumericUpDown.Value = product.Price;
        _isAvailableCheckBox.Checked = product.IsAvailable;

        // Select the user in the combo box
        for (int i = 0; i < _userComboBox.Items.Count; i++)
        {
            if (_userComboBox.Items[i] is CustomerInfo user && user.Id == product.UserId)
            {
                _userComboBox.SelectedIndex = i;
                break;
            }
        }
    }

    public Dictionary<string, string> ValidateForm()
    {
        var errors = new Dictionary<string, string>();

        if (string.IsNullOrWhiteSpace(_nameTextBox.Text))
            errors.Add("Name", "Product name is required");

        if (string.IsNullOrWhiteSpace(_descriptionTextBox.Text))
            errors.Add("Description", "Product description is required");

        if (_priceNumericUpDown.Value < 0)
            errors.Add("Price", "Price cannot be negative");

        if (_userComboBox.SelectedItem == null || !(_userComboBox.SelectedItem is CustomerInfo))
            errors.Add("User", "Please select a user");

        return errors;
    }

    public void ClearForm()
    {
        if (InvokeRequired)
        {
            Invoke(ClearForm);
            return;
        }

        _nameTextBox.Clear();
        _descriptionTextBox.Clear();
        _priceNumericUpDown.Value = 0;
        _isAvailableCheckBox.Checked = true;
        _userComboBox.SelectedIndex = -1;
    }

    public void SetAvailableUsers(IEnumerable<CustomerInfo> users)
    {
        if (InvokeRequired)
        {
            Invoke(() => SetAvailableUsers(users));
            return;
        }

        _userComboBox.Items.Clear();
        
        foreach (var user in users)
        {
            _userComboBox.Items.Add(user);
        }
    }

    /// <summary>
    /// Set available customers for selection (alias for SetAvailableUsers to match updated terminology)
    /// </summary>
    public void SetAvailableCustomers(IEnumerable<CustomerInfo> customers)
    {
        SetAvailableUsers(customers);
    }


}
