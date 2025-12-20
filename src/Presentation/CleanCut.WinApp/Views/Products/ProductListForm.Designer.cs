using System;
using System.Windows.Forms;
using System.Drawing;

namespace CleanCut.WinApp.Views.Products;

public partial class ProductListForm
{
    private DevExpress.XtraGrid.GridControl _gridControl = null!;
    private DevExpress.XtraGrid.Views.Grid.GridView _gridView = null!;
    private ComboBox _userFilterComboBox = null!;
    private Button _addButton = null!;
    private Button _editButton = null!;
    private Button _deleteButton = null!;
    private Button _refreshButton = null!;
    private Button _filterButton = null!;
    private Label _userFilterLabel = null!;

    private void InitializeComponent()
    {
        _gridControl = new DevExpress.XtraGrid.GridControl();
        _gridView = new DevExpress.XtraGrid.Views.Grid.GridView();
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
        
        // 
        // _filterButton
        // 
        _filterButton.Location = new Point(290, 12);
        _filterButton.Size = new Size(60, 23);
        _filterButton.Text = "Filter";
        _filterButton.UseVisualStyleBackColor = true;
        _filterButton.Enabled = false;
        
        // 
        // _gridControl and _gridView
        // 
        _gridControl.Location = new Point(12, 45);
        _gridControl.Size = new Size(776, 320);
        _gridControl.MainView = _gridView;
        _gridControl.ViewCollection.Add(_gridView);

        _gridView.OptionsBehavior.Editable = false;
        _gridView.OptionsSelection.EnableAppearanceFocusedCell = false;
        _gridView.OptionsView.ShowGroupPanel = false;
        _gridView.OptionsView.ShowIndicator = false;
        _gridView.Columns.AddVisible("Name", "Name");
        _gridView.Columns.AddVisible("Description", "Description");
        _gridView.Columns.AddVisible("Price", "Price");
        _gridView.Columns.AddVisible("Status", "Status");
        _gridView.Columns.AddVisible("Owner", "Owner");
        _gridView.Columns.AddVisible("Created", "Created");
        
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
        Controls.Add(_gridControl);
        Controls.Add(_addButton);
        Controls.Add(_editButton);
        Controls.Add(_deleteButton);
        Controls.Add(_refreshButton);
        Text = "Product Management - CleanCut";
        
        ResumeLayout(false);
        PerformLayout();
    }

}
