using System;
using System.Windows.Forms;
using System.Drawing;

namespace CleanCut.WinApp.Views.Countries;

public partial class CountryEditForm
{
    private TextBox _countryName = null!;
    private TextBox _countryId = null!;
    private Button _saveButton = null!;
    private Button _cancelButton = null!;

 

    private void InitializeComponent()
    {
        _countryName = new TextBox();
        _countryId = new TextBox();
        _saveButton = new Button();
        _cancelButton = new Button();
 
        SuspendLayout();
        // 
        // _countryName
        // 
        _countryName.Location = new Point(100, 55);
        _countryName.Name = "_countryName";
        _countryName.Size = new Size(250, 23);
        _countryName.TabIndex = 1;
        // 
        // _countryId
        // 
        _countryId.Location = new Point(100, 26);
        _countryId.Name = "_countryId";
        _countryId.Size = new Size(100, 23);
        _countryId.TabIndex = 3;
        // 
        // _saveButton
        // 
        _saveButton.Location = new Point(194, 100);
        _saveButton.Name = "_saveButton";
        _saveButton.Size = new Size(75, 23);
        _saveButton.TabIndex = 4;
        _saveButton.Text = "Save";
        // 
        // _cancelButton
        // 
        _cancelButton.Location = new Point(275, 100);
        _cancelButton.Name = "_cancelButton";
        _cancelButton.Size = new Size(75, 23);
        _cancelButton.TabIndex = 5;
        _cancelButton.Text = "Cancel";


        // 
        // CountryEditForm
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        ClientSize = new Size(370, 160);
        Controls.Add(_countryName);
        Controls.Add(_countryId);
        Controls.Add(_saveButton);
        Controls.Add(_cancelButton);
        Name = "CountryEditForm";
        Text = "Country";
        ResumeLayout(false);
        PerformLayout();
    }


}
