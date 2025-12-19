using System;
using System.Windows.Forms;
using System.Drawing;

namespace CleanCut.WinApp.Views.Customers;

public partial class CustomerListForm
{
    private ListView _listView = null!;
    private Button _addButton = null!;
    private Button _editButton = null!;
    private Button _deleteButton = null!;
    private Button _refreshButton = null!;

    private void InitializeComponent()
    {
        _listView = new ListView();
        _addButton = new Button();
        _editButton = new Button();
        _deleteButton = new Button();
        _refreshButton = new Button();
        
        SuspendLayout();
        
        // 
        // _listView
        // 
        _listView.View = View.Details;
        _listView.FullRowSelect = true;
        _listView.GridLines = true;
        _listView.MultiSelect = false;
        _listView.Location = new Point(12, 12);
        _listView.Size = new Size(776, 350);
        _listView.Columns.Add("First Name", 150);
        _listView.Columns.Add("Last Name", 150);
        _listView.Columns.Add("Email", 250);
        _listView.Columns.Add("Status", 100);
        _listView.Columns.Add("Created", 100);
        
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
        Controls.Add(_listView);
        Controls.Add(_addButton);
        Controls.Add(_editButton);
        Controls.Add(_deleteButton);
        Controls.Add(_refreshButton);
        Text = "Customer Management - CleanCut";
        
        ResumeLayout(false);
    }
}
