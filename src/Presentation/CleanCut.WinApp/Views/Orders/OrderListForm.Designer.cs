namespace CleanCut.WinApp.Views.Orders
{
    partial class OrderListForm
    {
        private DevExpress.XtraGrid.GridControl _gridControl;
        private DevExpress.XtraGrid.Views.Grid.GridView _gridView;
        private DevExpress.XtraEditors.SimpleButton _addButton;
        private DevExpress.XtraEditors.SimpleButton _editButton;
        private DevExpress.XtraEditors.SimpleButton _deleteButton;
        private DevExpress.XtraEditors.SimpleButton _refreshButton;
        private DevExpress.XtraEditors.SimpleButton _viewLineItemsButton;

        private void InitializeComponent()
        {
            _gridControl = new DevExpress.XtraGrid.GridControl();
            _gridView = new DevExpress.XtraGrid.Views.Grid.GridView();
            _addButton = new DevExpress.XtraEditors.SimpleButton();
            _editButton = new DevExpress.XtraEditors.SimpleButton();
            _deleteButton = new DevExpress.XtraEditors.SimpleButton();
            _refreshButton = new DevExpress.XtraEditors.SimpleButton();
            _viewLineItemsButton = new DevExpress.XtraEditors.SimpleButton();

            SuspendLayout();
            _gridControl.Location = new System.Drawing.Point(12, 12);
            _gridControl.Size = new System.Drawing.Size(900, 400);
            _gridControl.MainView = _gridView;
            _gridControl.ViewCollection.Add(_gridView);
            _gridView.OptionsBehavior.Editable = false;
            _gridView.OptionsSelection.EnableAppearanceFocusedCell = false;
            _gridView.OptionsView.ShowGroupPanel = false;
            _gridView.OptionsView.ShowIndicator = false;
            _gridView.Columns.AddVisible("OrderNumber", "Order #");
            _gridView.Columns.AddVisible("OrderDate", "Date");
            _gridView.Columns.AddVisible("CustomerName", "Customer");
            _gridView.Columns.AddVisible("StatusName", "Status");
            _gridView.Columns.AddVisible("TotalAmount", "Total");
            _gridView.Columns.AddVisible("TotalItemCount", "Items");
            _gridView.Columns.AddVisible("CreatedAt", "Created");
            _gridView.Columns.AddVisible("UpdatedAt", "Updated");
            _addButton.Text = "Add";
            _addButton.Location = new System.Drawing.Point(12, 420);
            _editButton.Text = "Edit";
            _editButton.Location = new System.Drawing.Point(100, 420);
            _deleteButton.Text = "Delete";
            _deleteButton.Location = new System.Drawing.Point(188, 420);
            _refreshButton.Text = "Refresh";
            _refreshButton.Location = new System.Drawing.Point(800, 420);
            _viewLineItemsButton.Text = "View Line Items";
            _viewLineItemsButton.Location = new System.Drawing.Point(276, 420);
            Controls.Add(_gridControl);
            Controls.Add(_addButton);
            Controls.Add(_editButton);
            Controls.Add(_deleteButton);
            Controls.Add(_refreshButton);
            Controls.Add(_viewLineItemsButton);
            ClientSize = new System.Drawing.Size(930, 470);
            Text = "Order Management";
            ResumeLayout(false);
        }
    }
}
