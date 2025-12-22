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
    private Button _saveLayoutButton = null!;
    private Button _loadLayoutButton = null!;

    private void InitializeComponent()
    {
        _gridControl = new GridControl();
        _addButton = new Button();
        _editButton = new Button();
        _deleteButton = new Button();
        _refreshButton = new Button();
        _saveLayoutButton = new Button();
        _loadLayoutButton = new Button();
        cardView1 = new DevExpress.XtraGrid.Views.Card.CardView();
        gridColumn1 = new DevExpress.XtraGrid.Columns.GridColumn();
        gridColumn2 = new DevExpress.XtraGrid.Columns.GridColumn();
        ((System.ComponentModel.ISupportInitialize) _gridControl).BeginInit();
        ((System.ComponentModel.ISupportInitialize) cardView1).BeginInit();
        SuspendLayout();
        // 
        // _gridControl
        // 
        _gridControl.Location = new Point(12, 12);
        _gridControl.MainView = cardView1;
        _gridControl.Name = "_gridControl";
        _gridControl.Size = new Size(600, 350);
        _gridControl.TabIndex = 0;
        _gridControl.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] { cardView1 });
        // 
        // _addButton
        // 
        _addButton.Location = new Point(12, 380);
        _addButton.Name = "_addButton";
        _addButton.Size = new Size(100, 30);
        _addButton.TabIndex = 1;
        _addButton.Text = "Add Country";
        // 
        // _editButton
        // 
        _editButton.Enabled = false;
        _editButton.Location = new Point(120, 380);
        _editButton.Name = "_editButton";
        _editButton.Size = new Size(100, 30);
        _editButton.TabIndex = 2;
        _editButton.Text = "Edit Country";
        // 
        // _deleteButton
        // 
        _deleteButton.Enabled = false;
        _deleteButton.Location = new Point(228, 380);
        _deleteButton.Name = "_deleteButton";
        _deleteButton.Size = new Size(100, 30);
        _deleteButton.TabIndex = 3;
        _deleteButton.Text = "Delete Country";
        // 
        // _refreshButton
        // 
        _refreshButton.Location = new Point(520, 380);
        _refreshButton.Name = "_refreshButton";
        _refreshButton.Size = new Size(100, 30);
        _refreshButton.TabIndex = 4;
        _refreshButton.Text = "Refresh";
        // 
        // _saveLayoutButton
        // 
        _saveLayoutButton.Location = new Point(336, 380);
        _saveLayoutButton.Name = "_saveLayoutButton";
        _saveLayoutButton.Size = new Size(90, 30);
        _saveLayoutButton.TabIndex = 5;
        _saveLayoutButton.Text = "Save Layout";
        // 
        // _loadLayoutButton
        // 
        _loadLayoutButton.Location = new Point(436, 380);
        _loadLayoutButton.Name = "_loadLayoutButton";
        _loadLayoutButton.Size = new Size(78, 30);
        _loadLayoutButton.TabIndex = 6;
        _loadLayoutButton.Text = "Load Layout";
        // 
        // cardView1
        // 
        cardView1.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] { gridColumn1, gridColumn2 });
        cardView1.GridControl = _gridControl;
        cardView1.Name = "cardView1";
        cardView1.OptionsBehavior.Editable = false;
        cardView1.VertScrollVisibility = DevExpress.XtraGrid.Views.Base.ScrollVisibility.Auto;
        // 
        // gridColumn1
        // 
        gridColumn1.Caption = "Name";
        gridColumn1.FieldName = "Name";
        gridColumn1.Name = "gridColumn1";
        gridColumn1.Visible = true;
        gridColumn1.VisibleIndex = 0;
        // 
        // gridColumn2
        // 
        gridColumn2.Caption = "Code";
        gridColumn2.FieldName = "Code";
        gridColumn2.Name = "gridColumn2";
        gridColumn2.Visible = true;
        gridColumn2.VisibleIndex = 1;
        // 
        // CountryListForm
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        ClientSize = new Size(640, 450);
        Controls.Add(_gridControl);
        Controls.Add(_addButton);
        Controls.Add(_editButton);
        Controls.Add(_deleteButton);
        Controls.Add(_refreshButton);
        Controls.Add(_saveLayoutButton);
        Controls.Add(_loadLayoutButton);
        Name = "CountryListForm";
        Text = "Country Management - CleanCut";
        ((System.ComponentModel.ISupportInitialize) _gridControl).EndInit();
        ((System.ComponentModel.ISupportInitialize) cardView1).EndInit();
        ResumeLayout(false);
    }

    private DevExpress.XtraGrid.Views.Card.CardView cardView1;
    private DevExpress.XtraGrid.Columns.GridColumn gridColumn1;
    private DevExpress.XtraGrid.Columns.GridColumn gridColumn2;
}
