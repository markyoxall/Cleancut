namespace CleanCut.WinApp.Views.Orders
{
    partial class OrderLineItemListForm
    {
        private DevExpress.XtraGrid.GridControl _gridControl;
        private DevExpress.XtraGrid.Views.Grid.GridView _gridView;
        private DevExpress.XtraEditors.SimpleButton _addButton;
        private DevExpress.XtraEditors.SimpleButton _editButton;
        private DevExpress.XtraEditors.SimpleButton _deleteButton;
        private DevExpress.XtraEditors.SimpleButton _refreshButton;

        private void InitializeComponent()
        {
            _gridControl = new DevExpress.XtraGrid.GridControl();
            _gridView = new DevExpress.XtraGrid.Views.Grid.GridView();
            _addButton = new DevExpress.XtraEditors.SimpleButton();
            _editButton = new DevExpress.XtraEditors.SimpleButton();
            _deleteButton = new DevExpress.XtraEditors.SimpleButton();
            _refreshButton = new DevExpress.XtraEditors.SimpleButton();

            SuspendLayout();
            _gridControl.Location = new System.Drawing.Point(12, 12);
            _gridControl.Size = new System.Drawing.Size(700, 300);
            _gridControl.MainView = _gridView;
            _gridControl.ViewCollection.Add(_gridView);
            _gridView.OptionsBehavior.Editable = false;
            _gridView.OptionsSelection.EnableAppearanceFocusedCell = false;
            _gridView.OptionsView.ShowGroupPanel = false;
            _gridView.OptionsView.ShowIndicator = false;
            _gridView.Columns.AddVisible("ProductName", "Product");
            _gridView.Columns.AddVisible("UnitPrice", "Unit Price");
            _gridView.Columns.AddVisible("Quantity", "Quantity");
            _gridView.Columns.AddVisible("LineTotal", "Line Total");
            _gridView.Columns.AddVisible("CreatedAt", "Created");
            _gridView.Columns.AddVisible("UpdatedAt", "Updated");
            _addButton.Text = "Add";
            _addButton.Location = new System.Drawing.Point(12, 320);
            _editButton.Text = "Edit";
            _editButton.Location = new System.Drawing.Point(100, 320);
            _deleteButton.Text = "Delete";
            _deleteButton.Location = new System.Drawing.Point(188, 320);
            _refreshButton.Text = "Refresh";
            _refreshButton.Location = new System.Drawing.Point(612, 320);
            Controls.Add(_gridControl);
            Controls.Add(_addButton);
            Controls.Add(_editButton);
            Controls.Add(_deleteButton);
            Controls.Add(_refreshButton);
            ClientSize = new System.Drawing.Size(730, 370);
            Text = "Order Line Items";
            ResumeLayout(false);
        }
    }
}
