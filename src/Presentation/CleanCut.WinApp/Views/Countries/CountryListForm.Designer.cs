using System;
using System.Windows.Forms;
using System.Drawing;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;

namespace CleanCut.WinApp.Views.Countries;

public partial class CountryListForm
{
    private GridControl _gridControl = null!;
    private GridView _gridView = null!;
    private Button _addButton = null!;
    private Button _editButton = null!;
    private Button _deleteButton = null!;
    private Button _refreshButton = null!;

    private void InitializeComponent()
    {
        _gridControl = new GridControl();
        _gridView = new GridView();
        _addButton = new Button();
        _editButton = new Button();
        _deleteButton = new Button();
        _refreshButton = new Button();

        SuspendLayout();

        // GridControl and GridView setup
        _gridControl.Location = new Point(12, 12);
        _gridControl.Size = new Size(600, 350);
        _gridControl.MainView = _gridView;
        _gridControl.ViewCollection.Add(_gridView);

        _gridView.OptionsBehavior.Editable = false;
        _gridView.OptionsSelection.EnableAppearanceFocusedCell = false;
        _gridView.OptionsView.ShowGroupPanel = false;
        _gridView.OptionsView.ShowIndicator = false;
        _gridView.Columns.AddVisible("Name", "Name");
        _gridView.Columns.AddVisible("Code", "Code");

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
        Controls.Add(_gridControl);
        Controls.Add(_addButton);
        Controls.Add(_editButton);
        Controls.Add(_deleteButton);
        Controls.Add(_refreshButton);
        Text = "Country Management - CleanCut";

        ResumeLayout(false);
    }
}
