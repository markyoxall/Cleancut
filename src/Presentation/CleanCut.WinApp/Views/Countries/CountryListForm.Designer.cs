using System;
using System.Windows.Forms;
using System.Drawing;

namespace CleanCut.WinApp.Views.Countries;

public partial class CountryListForm
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

        _listView.View = View.Details;
        _listView.FullRowSelect = true;
        _listView.GridLines = true;
        _listView.MultiSelect = false;
        _listView.Location = new Point(12, 12);
        _listView.Size = new Size(600, 350);
        _listView.Columns.Add("Name", 200);
        _listView.Columns.Add("Code", 100);

        _addButton.Location = new Point(12, 380);
        _addButton.Size = new Size(100, 30);
        _addButton.Text = "Add Country";

        _editButton.Location = new Point(120, 380);
        _editButton.Size = new Size(100, 30);
        _editButton.Text = "Edit Country";
        _editButton.Enabled = false;

        _deleteButton.Location = new Point(228, 380);
        _deleteButton.Size = new Size(100, 30);
        _deleteButton.Text = "Delete Country";
        _deleteButton.Enabled = false;

        _refreshButton.Location = new Point(520, 380);
        _refreshButton.Size = new Size(100, 30);
        _refreshButton.Text = "Refresh";

        ClientSize = new Size(640, 450);
        Controls.Add(_listView);
        Controls.Add(_addButton);
        Controls.Add(_editButton);
        Controls.Add(_deleteButton);
        Controls.Add(_refreshButton);
        Text = "Country Management - CleanCut";

        ResumeLayout(false);
    }
}
