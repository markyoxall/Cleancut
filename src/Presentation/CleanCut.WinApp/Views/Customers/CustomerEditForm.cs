using CleanCut.WinApp.MVP;

namespace CleanCut.WinApp.Views.Customers;

/// <summary>
/// Customer Edit Form implementing MVP pattern
/// </summary>
public partial class CustomerEditForm : BaseForm, ICustomerEditView
{
    public event EventHandler? SaveRequested;
    public event EventHandler? CancelRequested;



    public CustomerEditForm()
    {
        InitializeComponent();
        SetupEventHandlers();
    }

    private void SetupEventHandlers()
    {
        _saveButton.Click += (s, e) => SaveRequested?.Invoke(this, EventArgs.Empty);
        _cancelButton.Click += (s, e) => CancelRequested?.Invoke(this, EventArgs.Empty);
    }

    public CustomerEditViewModel GetCustomerData()
    {
        return new CustomerEditViewModel
        {
            FirstName = _firstNameTextBox.Text.Trim(),
            LastName = _lastNameTextBox.Text.Trim(),
            Email = _emailTextBox.Text.Trim(),
            IsActive = _isActiveCheckBox.Checked
        };
    }

    public void SetCustomerData(CustomerEditViewModel user)
    {
        if (InvokeRequired)
        {
            Invoke(() => SetCustomerData(user));
            return;
        }

        _firstNameTextBox.Text = user.FirstName;
        _lastNameTextBox.Text = user.LastName;
        _emailTextBox.Text = user.Email;
        _isActiveCheckBox.Checked = user.IsActive;
    }

    public Dictionary<string, string> ValidateForm()
    {
        var errors = new Dictionary<string, string>();

        if (string.IsNullOrWhiteSpace(_firstNameTextBox.Text))
            errors.Add("FirstName", "First name is required");

        if (string.IsNullOrWhiteSpace(_lastNameTextBox.Text))
            errors.Add("LastName", "Last name is required");

        if (string.IsNullOrWhiteSpace(_emailTextBox.Text))
            errors.Add("Email", "Email is required");
        else if (!IsValidEmail(_emailTextBox.Text))
            errors.Add("Email", "Please enter a valid email address");

        return errors;
    }

    public void ClearForm()
    {
        if (InvokeRequired)
        {
            Invoke(ClearForm);
            return;
        }

        _firstNameTextBox.Clear();
        _lastNameTextBox.Clear();
        _emailTextBox.Clear();
        _isActiveCheckBox.Checked = true;
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }


}
