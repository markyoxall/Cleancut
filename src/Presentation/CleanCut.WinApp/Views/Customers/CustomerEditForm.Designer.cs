using System;
using System.Windows.Forms;
using System.Drawing;

namespace CleanCut.WinApp.Views.Customers;

public partial class CustomerEditForm
{
    private TextBox _firstNameTextBox = null!;
    private TextBox _lastNameTextBox = null!;
    private TextBox _emailTextBox = null!;
    private CheckBox _isActiveCheckBox = null!;
    private Button _saveButton = null!;
    private Button _cancelButton = null!;
    private Label _emailLabel = null!;

    private void InitializeComponent()
    {
        _firstNameTextBox = new TextBox();
        _lastNameTextBox = new TextBox();
        _emailLabel = new Label();
        _emailTextBox = new TextBox();
        _isActiveCheckBox = new CheckBox();
        _saveButton = new Button();
        _cancelButton = new Button();
        _firstNameLabel = new Label();
        _lastNameLabel = new Label();
        SuspendLayout();
        // 
        // _firstNameTextBox
        // 
        _firstNameTextBox.Location = new Point(100, 12);
        _firstNameTextBox.Name = "_firstNameTextBox";
        _firstNameTextBox.Size = new Size(200, 23);
        _firstNameTextBox.TabIndex = 1;
        // 
        // _lastNameTextBox
        // 
        _lastNameTextBox.Location = new Point(100, 41);
        _lastNameTextBox.Name = "_lastNameTextBox";
        _lastNameTextBox.Size = new Size(200, 23);
        _lastNameTextBox.TabIndex = 3;
        // 
        // _emailLabel
        // 
        _emailLabel.AutoSize = true;
        _emailLabel.Location = new Point(12, 73);
        _emailLabel.Name = "_emailLabel";
        _emailLabel.Size = new Size(39, 15);
        _emailLabel.TabIndex = 4;
        _emailLabel.Text = "Email:";
        // 
        // _emailTextBox
        // 
        _emailTextBox.Location = new Point(100, 70);
        _emailTextBox.Name = "_emailTextBox";
        _emailTextBox.Size = new Size(200, 23);
        _emailTextBox.TabIndex = 5;
        // 
        // _isActiveCheckBox
        // 
        _isActiveCheckBox.AutoSize = true;
        _isActiveCheckBox.Checked = true;
        _isActiveCheckBox.CheckState = CheckState.Checked;
        _isActiveCheckBox.Location = new Point(100, 99);
        _isActiveCheckBox.Name = "_isActiveCheckBox";
        _isActiveCheckBox.Size = new Size(59, 19);
        _isActiveCheckBox.TabIndex = 6;
        _isActiveCheckBox.Text = "Active";
        _isActiveCheckBox.UseVisualStyleBackColor = true;
        // 
        // _saveButton
        // 
        _saveButton.Location = new Point(144, 140);
        _saveButton.Name = "_saveButton";
        _saveButton.Size = new Size(75, 23);
        _saveButton.TabIndex = 7;
        _saveButton.Text = "Save";
        _saveButton.UseVisualStyleBackColor = true;
        // 
        // _cancelButton
        // 
        _cancelButton.Location = new Point(225, 140);
        _cancelButton.Name = "_cancelButton";
        _cancelButton.Size = new Size(75, 23);
        _cancelButton.TabIndex = 8;
        _cancelButton.Text = "Cancel";
        _cancelButton.UseVisualStyleBackColor = true;
        // 
        // _firstNameLabel
        // 
        _firstNameLabel.AutoSize = true;
        _firstNameLabel.Location = new Point(12, 15);
        _firstNameLabel.Name = "_firstNameLabel";
        _firstNameLabel.Size = new Size(67, 15);
        _firstNameLabel.TabIndex = 0;
        _firstNameLabel.Text = "First Name:";
        // 
        // _lastNameLabel
        // 
        _lastNameLabel.AutoSize = true;
        _lastNameLabel.Location = new Point(12, 44);
        _lastNameLabel.Name = "_lastNameLabel";
        _lastNameLabel.Size = new Size(66, 15);
        _lastNameLabel.TabIndex = 2;
        _lastNameLabel.Text = "Last Name:";
        // 
        // CustomerEditForm
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
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
        Name = "CustomerEditForm";
        StartPosition = FormStartPosition.CenterParent;
        Text = "Customer Details";
        ResumeLayout(false);
        PerformLayout();
    }

    private Label _firstNameLabel;
    private Label _lastNameLabel;
}
