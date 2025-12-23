namespace CleanCut.WinApp.Views.Orders
{
    partial class OrderListForm
    {
        private DevExpress.XtraEditors.SimpleButton _addButton;
        private DevExpress.XtraEditors.SimpleButton _editButton;
        private DevExpress.XtraEditors.SimpleButton _deleteButton;
        private DevExpress.XtraEditors.SimpleButton _refreshButton;
        private DevExpress.XtraEditors.SimpleButton _viewLineItemsButton;

        private void InitializeComponent()
        {
            _addButton = new DevExpress.XtraEditors.SimpleButton();
            _editButton = new DevExpress.XtraEditors.SimpleButton();
            _deleteButton = new DevExpress.XtraEditors.SimpleButton();
            _refreshButton = new DevExpress.XtraEditors.SimpleButton();
            _viewLineItemsButton = new DevExpress.XtraEditors.SimpleButton();
            _gridControl = new DevExpress.XtraGrid.GridControl();
            gridView1 = new DevExpress.XtraGrid.Views.Grid.GridView();
            ((System.ComponentModel.ISupportInitialize) _gridControl).BeginInit();
            ((System.ComponentModel.ISupportInitialize) gridView1).BeginInit();
            SuspendLayout();
            // 
            // _addButton
            // 
            _addButton.Location = new Point(12, 420);
            _addButton.Name = "_addButton";
            _addButton.Size = new Size(75, 23);
            _addButton.TabIndex = 1;
            _addButton.Text = "Add";
            // 
            // _editButton
            // 
            _editButton.Location = new Point(100, 420);
            _editButton.Name = "_editButton";
            _editButton.Size = new Size(75, 23);
            _editButton.TabIndex = 2;
            _editButton.Text = "Edit";
            // 
            // _deleteButton
            // 
            _deleteButton.Location = new Point(188, 420);
            _deleteButton.Name = "_deleteButton";
            _deleteButton.Size = new Size(75, 23);
            _deleteButton.TabIndex = 3;
            _deleteButton.Text = "Delete";
            // 
            // _refreshButton
            // 
            _refreshButton.Location = new Point(800, 420);
            _refreshButton.Name = "_refreshButton";
            _refreshButton.Size = new Size(75, 23);
            _refreshButton.TabIndex = 4;
            _refreshButton.Text = "Refresh";
            // 
            // _viewLineItemsButton
            // 
            _viewLineItemsButton.Location = new Point(276, 420);
            _viewLineItemsButton.Name = "_viewLineItemsButton";
            _viewLineItemsButton.Size = new Size(75, 23);
            _viewLineItemsButton.TabIndex = 5;
            _viewLineItemsButton.Text = "View Line Items";
            // 
            // _gridControl
            // 
            _gridControl.Location = new Point(86, 76);
            _gridControl.MainView = gridView1;
            _gridControl.Name = "_gridControl";
            _gridControl.Size = new Size(400, 200);
            _gridControl.TabIndex = 6;
            _gridControl.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] { gridView1 });
            // 
            // gridView1
            // 
            gridView1.GridControl = _gridControl;
            gridView1.Name = "gridView1";
            // 
            // OrderListForm
            // 
            ClientSize = new Size(930, 470);
            Controls.Add(_gridControl);
            Controls.Add(_addButton);
            Controls.Add(_editButton);
            Controls.Add(_deleteButton);
            Controls.Add(_refreshButton);
            Controls.Add(_viewLineItemsButton);
            Name = "OrderListForm";
            Text = "Order Management";
            ((System.ComponentModel.ISupportInitialize) _gridControl).EndInit();
            ((System.ComponentModel.ISupportInitialize) gridView1).EndInit();
            ResumeLayout(false);
        }

        private DevExpress.XtraGrid.GridControl _gridControl;
        private DevExpress.XtraGrid.Views.Grid.GridView gridView1;
    }
}
