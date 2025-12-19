using System;
using System.Windows.Forms;
using System.Drawing;

namespace CleanCut.WinApp.Views.Products;

public partial class ProductEditForm
{
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
        ((System.ComponentModel.ISupportInitialize)_priceNumericUpDown).BeginInit();
        SuspendLayout();
        // 
        // _nameLabel
        // 
        _nameLabel.AutoSize = true;
        _nameLabel.Location = new Point(12, 15);
        _nameLabel.Name = "_nameLabel";
        _nameLabel.Size = new Size(42, 15);
        _nameLabel.TabIndex = 0;
        _nameLabel.Text = "Name:";
        // 
        // _nameTextBox
        // 
        _nameTextBox.Location = new Point(100, 12);
        _nameTextBox.Name = "_nameTextBox";
        _nameTextBox.Size = new Size(250, 23);
        _nameTextBox.TabIndex = 1;
        // 
        // _descriptionLabel
        // 
        _descriptionLabel.AutoSize = true;
        _descriptionLabel.Location = new Point(12, 44);
        _descriptionLabel.Name = "_descriptionLabel";
        _descriptionLabel.Size = new Size(70, 15);
        _descriptionLabel.TabIndex = 2;
        _descriptionLabel.Text = "Description:";
        // 
        // _descriptionTextBox
        // 
        _descriptionTextBox.Location = new Point(100, 41);
        _descriptionTextBox.Multiline = true;
        _descriptionTextBox.Name = "_descriptionTextBox";
        _descriptionTextBox.ScrollBars = ScrollBars.Vertical;
        _descriptionTextBox.Size = new Size(250, 60);
        _descriptionTextBox.TabIndex = 3;
        // 
        // _priceLabel
        // 
        _priceLabel.AutoSize = true;
        _priceLabel.Location = new Point(12, 110);
        _priceLabel.Name = "_priceLabel";
        _priceLabel.Size = new Size(36, 15);
        _priceLabel.TabIndex = 4;
        _priceLabel.Text = "Price:";
        // 
        // _priceNumericUpDown
        // 
        _priceNumericUpDown.DecimalPlaces = 2;
        _priceNumericUpDown.Location = new Point(100, 107);
        _priceNumericUpDown.Maximum = new decimal(new int[] { 999999, 0, 0, 0 });
        _priceNumericUpDown.Name = "_priceNumericUpDown";
        _priceNumericUpDown.Size = new Size(120, 23);
        _priceNumericUpDown.TabIndex = 5;
        // 
        // _userLabel
        // 
        _userLabel.AutoSize = true;
        _userLabel.Location = new Point(12, 139);
        _userLabel.Name = "_userLabel";
        _userLabel.Size = new Size(33, 15);
        _userLabel.TabIndex = 6;
        _userLabel.Text = "User:";
        // 
        // _userComboBox
        // 
        _userComboBox.DisplayMember = "FullName";
        _userComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        _userComboBox.Location = new Point(100, 136);
        _userComboBox.Name = "_userComboBox";
        _userComboBox.Size = new Size(200, 23);
        _userComboBox.TabIndex = 7;
        // 
        // _isAvailableCheckBox
        // 
        _isAvailableCheckBox.AutoSize = true;
        _isAvailableCheckBox.Checked = true;
        _isAvailableCheckBox.CheckState = CheckState.Checked;
        _isAvailableCheckBox.Location = new Point(100, 165);
        _isAvailableCheckBox.Name = "_isAvailableCheckBox";
        _isAvailableCheckBox.Size = new Size(74, 19);
        _isAvailableCheckBox.TabIndex = 8;
        _isAvailableCheckBox.Text = "Available";
        _isAvailableCheckBox.UseVisualStyleBackColor = true;
        // 
        // _saveButton
        // 
        _saveButton.Location = new Point(194, 200);
        _saveButton.Name = "_saveButton";
        _saveButton.Size = new Size(75, 23);
        _saveButton.TabIndex = 9;
        _saveButton.Text = "Save";
        _saveButton.UseVisualStyleBackColor = true;
        // 
        // _cancelButton
        // 
        _cancelButton.Location = new Point(275, 200);
        _cancelButton.Name = "_cancelButton";
        _cancelButton.Size = new Size(75, 23);
        _cancelButton.TabIndex = 10;
        _cancelButton.Text = "Cancel";
        _cancelButton.UseVisualStyleBackColor = true;
        // 
        // ProductEditForm
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
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
        Name = "ProductEditForm";
        StartPosition = FormStartPosition.CenterParent;
        Text = "Product";
        ((System.ComponentModel.ISupportInitialize)_priceNumericUpDown).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }
}
