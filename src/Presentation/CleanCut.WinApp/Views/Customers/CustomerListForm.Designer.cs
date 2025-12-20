using System;
using System.Windows.Forms;
using System.Drawing;

namespace CleanCut.WinApp.Views.Customers;

public partial class CustomerListForm
{
    private DevExpress.XtraGrid.GridControl _gridControl = null!;
    private DevExpress.XtraGrid.Views.Grid.GridView _gridView = null!;
    private Button _addButton = null!;
    private Button _editButton = null!;
    private Button _deleteButton = null!;
    private Button _refreshButton = null!;

    private void InitializeComponent()
    {
        _gridControl = new DevExpress.XtraGrid.GridControl();
        _gridView = new DevExpress.XtraGrid.Views.Grid.GridView();
        _addButton = new Button();
        _editButton = new Button();
        _deleteButton = new Button();
        _refreshButton = new Button();
        
        SuspendLayout();
        
        // 
        // _gridControl and _gridView
        // 
        _gridControl.Location = new Point(12, 12);
        _gridControl.Size = new Size(776, 350);
        _gridControl.MainView = _gridView;
        _gridControl.ViewCollection.Add(_gridView);

        _gridView.OptionsBehavior.Editable = false;
        _gridView.OptionsSelection.EnableAppearanceFocusedCell = false;
        _gridView.OptionsView.ShowGroupPanel = false;
        _gridView.OptionsView.ShowIndicator = false;
        _gridView.Columns.AddVisible("FirstName", "First Name");
        _gridView.Columns.AddVisible("LastName", "Last Name");
        _gridView.Columns.AddVisible("Email", "Email");
        _gridView.Columns.AddVisible("Status", "Status");
        _gridView.Columns.AddVisible("Created", "Created");
        
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
        Controls.Add(_gridControl);
        Controls.Add(_addButton);
        Controls.Add(_editButton);
        Controls.Add(_deleteButton);
        Controls.Add(_refreshButton);
        Text = "Customer Management - CleanCut";
        
        ResumeLayout(false);
    }
}
