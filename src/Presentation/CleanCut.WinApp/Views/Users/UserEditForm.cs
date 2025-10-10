using CleanCut.WinApp.MVP;

namespace CleanCut.WinApp.Views.Users;

/// <summary>
/// User Edit Form implementing MVP pattern
/// </summary>
public partial class UserEditForm : BaseForm, IUserEditView
{
    public event EventHandler? SaveRequested;
    public event EventHandler? CancelRequested;

    private TextBox _firstNameTextBox = null!;
    private TextBox _lastNameTextBox = null!;
    private TextBox _emailTextBox = null!;
    private CheckBox _isActiveCheckBox = null!;
    private Button _saveButton = null!;
    private Button _cancelButton = null!;
    private Label _firstNameLabel = null!;
    private Label _lastNameLabel = null!;
    private Label _emailLabel = null!;

    public UserEditForm()
    {
        InitializeComponent();
        SetupEventHandlers();
    }

    private void SetupEventHandlers()
    {
        _saveButton.Click += (s, e) => SaveRequested?.Invoke(this, EventArgs.Empty);
        _cancelButton.Click += (s, e) => CancelRequested?.Invoke(this, EventArgs.Empty);
    }

    public UserEditModel GetUserData()
    {
        return new UserEditModel
        {
            FirstName = _firstNameTextBox.Text.Trim(),
            LastName = _lastNameTextBox.Text.Trim(),
            Email = _emailTextBox.Text.Trim(),
            IsActive = _isActiveCheckBox.Checked
        };
    }

    public void SetUserData(UserEditModel user)
    {
        if (InvokeRequired)
        {
            Invoke(() => SetUserData(user));
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

    private void InitializeComponent()
    {
        _firstNameLabel = new Label();
        _firstNameTextBox = new TextBox();
        _lastNameLabel = new Label();
        _lastNameTextBox = new TextBox();
        _emailLabel = new Label();
        _emailTextBox = new TextBox();
        _isActiveCheckBox = new CheckBox();
        _saveButton = new Button();
        _cancelButton = new Button();
        
        SuspendLayout();
        
        // 
        // _firstNameLabel
        // 
        _firstNameLabel.AutoSize = true;
        _firstNameLabel.Location = new Point(12, 15);
        _firstNameLabel.Name = "_firstNameLabel";
        _firstNameLabel.Size = new Size(67, 15);
        _firstNameLabel.Text = "First Name:";
        
        // 
        // _firstNameTextBox
        // 
        _firstNameTextBox.Location = new Point(100, 12);
        _firstNameTextBox.Name = "_firstNameTextBox";
        _firstNameTextBox.Size = new Size(200, 23);
        
        // 
        // _lastNameLabel
        // 
        _lastNameLabel.AutoSize = true;
        _lastNameLabel.Location = new Point(12, 44);
        _lastNameLabel.Name = "_lastNameLabel";
        _lastNameLabel.Size = new Size(66, 15);
        _lastNameLabel.Text = "Last Name:";
        
        // 
        // _lastNameTextBox
        // 
        _lastNameTextBox.Location = new Point(100, 41);
        _lastNameTextBox.Name = "_lastNameTextBox";
        _lastNameTextBox.Size = new Size(200, 23);
        
        // 
        // _emailLabel
        // 
        _emailLabel.AutoSize = true;
        _emailLabel.Location = new Point(12, 73);
        _emailLabel.Name = "_emailLabel";
        _emailLabel.Size = new Size(39, 15);
        _emailLabel.Text = "Email:";
        
        // 
        // _emailTextBox
        // 
        _emailTextBox.Location = new Point(100, 70);
        _emailTextBox.Name = "_emailTextBox";
        _emailTextBox.Size = new Size(200, 23);
        
        // 
        // _isActiveCheckBox
        // 
        _isActiveCheckBox.AutoSize = true;
        _isActiveCheckBox.Checked = true;
        _isActiveCheckBox.CheckState = CheckState.Checked;
        _isActiveCheckBox.Location = new Point(100, 99);
        _isActiveCheckBox.Name = "_isActiveCheckBox";
        _isActiveCheckBox.Size = new Size(59, 19);
        _isActiveCheckBox.Text = "Active";
        _isActiveCheckBox.UseVisualStyleBackColor = true;
        
        // 
        // _saveButton
        // 
        _saveButton.Location = new Point(144, 140);
        _saveButton.Name = "_saveButton";
        _saveButton.Size = new Size(75, 23);
        _saveButton.Text = "Save";
        _saveButton.UseVisualStyleBackColor = true;
        
        // 
        // _cancelButton
        // 
        _cancelButton.Location = new Point(225, 140);
        _cancelButton.Name = "_cancelButton";
        _cancelButton.Size = new Size(75, 23);
        _cancelButton.Text = "Cancel";
        _cancelButton.UseVisualStyleBackColor = true;
        
        // 
        // UserEditForm
        // 
        ClientSize = new Size(320, 180);
        Controls.Add(_firstNameLabel);
        Controls.Add(_firstNameTextBox);
        Controls.Add(_lastNameLabel);
        Controls.Add(_lastNameTextBox);
        Controls.Add(_emailLabel);
        Controls.Add(_emailTextBox);
        Controls.Add(_isActiveCheckBox);
        Controls.Add(_saveButton);
        Controls.Add(_cancelButton);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        Text = "User Details";
        
        ResumeLayout(false);
        PerformLayout();
    }
}