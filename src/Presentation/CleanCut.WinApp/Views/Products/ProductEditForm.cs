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

    private TextBox _nameTextBox = null!;
    private TextBox _descriptionTextBox = null!;
    private NumericUpDown _priceNumericUpDown = null!;
    private CheckBox _isAvailableCheckBox = null!;
    private ComboBox _userComboBox = null!;
    private Button _saveButton = null!;
    private Button _cancelButton = null!;
    private Label _nameLabel = null!;
    private Label _descriptionLabel = null!;
    private Label _priceLabel = null!;
    private Label _userLabel = null!;

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

    public ProductEditModel GetProductData()
    {
        var selectedUser = _userComboBox.SelectedItem as CustomerInfo;
        
        return new ProductEditModel
        {
            Name = _nameTextBox.Text.Trim(),
            Description = _descriptionTextBox.Text.Trim(),
            Price = _priceNumericUpDown.Value,
            IsAvailable = _isAvailableCheckBox.Checked,
            UserId = selectedUser?.Id ?? Guid.Empty
        };
    }

    public void SetProductData(ProductEditModel product)
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

    private void InitializeComponent()
    {
        _nameLabel = new Label();
        _nameTextBox = new TextBox();
        _descriptionLabel = new Label();
        _descriptionTextBox = new TextBox();
        _priceLabel = new Label();
        _priceNumericUpDown = new NumericUpDown();
        _userLabel = new Label();
        _userComboBox = new ComboBox();
        _isAvailableCheckBox = new CheckBox();
        _saveButton = new Button();
        _cancelButton = new Button();
        
        SuspendLayout();
        
        // 
        // _nameLabel
        // 
        _nameLabel.AutoSize = true;
        _nameLabel.Location = new Point(12, 15);
        _nameLabel.Name = "_nameLabel";
        _nameLabel.Size = new Size(42, 15);
        _nameLabel.Text = "Name:";
        
        // 
        // _nameTextBox
        // 
        _nameTextBox.Location = new Point(100, 12);
        _nameTextBox.Name = "_nameTextBox";
        _nameTextBox.Size = new Size(250, 23);
        
        // 
        // _descriptionLabel
        // 
        _descriptionLabel.AutoSize = true;
        _descriptionLabel.Location = new Point(12, 44);
        _descriptionLabel.Name = "_descriptionLabel";
        _descriptionLabel.Size = new Size(70, 15);
        _descriptionLabel.Text = "Description:";
        
        // 
        // _descriptionTextBox
        // 
        _descriptionTextBox.Location = new Point(100, 41);
        _descriptionTextBox.Name = "_descriptionTextBox";
        _descriptionTextBox.Size = new Size(250, 60);
        _descriptionTextBox.Multiline = true;
        _descriptionTextBox.ScrollBars = ScrollBars.Vertical;
        
        // 
        // _priceLabel
        // 
        _priceLabel.AutoSize = true;
        _priceLabel.Location = new Point(12, 110);
        _priceLabel.Name = "_priceLabel";
        _priceLabel.Size = new Size(36, 15);
        _priceLabel.Text = "Price:";
        
        // 
        // _priceNumericUpDown
        // 
        _priceNumericUpDown.DecimalPlaces = 2;
        _priceNumericUpDown.Location = new Point(100, 107);
        _priceNumericUpDown.Maximum = 999999;
        _priceNumericUpDown.Name = "_priceNumericUpDown";
        _priceNumericUpDown.Size = new Size(120, 23);
        
        // 
        // _userLabel
        // 
        _userLabel.AutoSize = true;
        _userLabel.Location = new Point(12, 139);
        _userLabel.Name = "_userLabel";
        _userLabel.Size = new Size(33, 15);
        _userLabel.Text = "User:";
        
        // 
        // _userComboBox
        // 
        _userComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        _userComboBox.Location = new Point(100, 136);
        _userComboBox.Name = "_userComboBox";
        _userComboBox.Size = new Size(200, 23);
        _userComboBox.DisplayMember = "FullName";
        
        // 
        // _isAvailableCheckBox
        // 
        _isAvailableCheckBox.AutoSize = true;
        _isAvailableCheckBox.Checked = true;
        _isAvailableCheckBox.CheckState = CheckState.Checked;
        _isAvailableCheckBox.Location = new Point(100, 165);
        _isAvailableCheckBox.Name = "_isAvailableCheckBox";
        _isAvailableCheckBox.Size = new Size(72, 19);
        _isAvailableCheckBox.Text = "Available";
        _isAvailableCheckBox.UseVisualStyleBackColor = true;
        
        // 
        // _saveButton
        // 
        _saveButton.Location = new Point(194, 200);
        _saveButton.Name = "_saveButton";
        _saveButton.Size = new Size(75, 23);
        _saveButton.Text = "Save";
        _saveButton.UseVisualStyleBackColor = true;
        
        // 
        // _cancelButton
        // 
        _cancelButton.Location = new Point(275, 200);
        _cancelButton.Name = "_cancelButton";
        _cancelButton.Size = new Size(75, 23);
        _cancelButton.Text = "Cancel";
        _cancelButton.UseVisualStyleBackColor = true;
        
        // 
        // ProductEditForm
        // 
        ClientSize = new Size(370, 240);
        Controls.Add(_nameLabel);
        Controls.Add(_nameTextBox);
        Controls.Add(_descriptionLabel);
        Controls.Add(_descriptionTextBox);
        Controls.Add(_priceLabel);
        Controls.Add(_priceNumericUpDown);
        Controls.Add(_userLabel);
        Controls.Add(_userComboBox);
        Controls.Add(_isAvailableCheckBox);
        Controls.Add(_saveButton);
        Controls.Add(_cancelButton);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        Text = "Product Details";
        
        ResumeLayout(false);
        PerformLayout();
    }
}