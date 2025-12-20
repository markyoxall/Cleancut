namespace CleanCut.WinApp.Views.Orders
{
    partial class OrderLineItemEditForm
    {
        private DevExpress.XtraEditors.TextEdit _productName;
        private DevExpress.XtraEditors.TextEdit _unitPrice;
        private DevExpress.XtraEditors.SpinEdit _quantity;
        private DevExpress.XtraEditors.SimpleButton _saveButton;
        private DevExpress.XtraEditors.SimpleButton _cancelButton;

        private void InitializeComponent()
        {
            _productName = new DevExpress.XtraEditors.TextEdit();
            _unitPrice = new DevExpress.XtraEditors.TextEdit();
            _quantity = new DevExpress.XtraEditors.SpinEdit();
            _saveButton = new DevExpress.XtraEditors.SimpleButton();
            _cancelButton = new DevExpress.XtraEditors.SimpleButton();

            SuspendLayout();
            _productName.Location = new System.Drawing.Point(12, 12);
            _productName.Size = new System.Drawing.Size(400, 22);
            _unitPrice.Location = new System.Drawing.Point(12, 50);
            _unitPrice.Size = new System.Drawing.Size(200, 22);
            _quantity.Location = new System.Drawing.Point(12, 88);
            _quantity.Size = new System.Drawing.Size(100, 22);
            _saveButton.Text = "Save";
            _saveButton.Location = new System.Drawing.Point(12, 130);
            _cancelButton.Text = "Cancel";
            _cancelButton.Location = new System.Drawing.Point(100, 130);
            Controls.Add(_productName);
            Controls.Add(_unitPrice);
            Controls.Add(_quantity);
            Controls.Add(_saveButton);
            Controls.Add(_cancelButton);
            Text = "Edit Line Item";
            ClientSize = new System.Drawing.Size(430, 180);
            ResumeLayout(false);
        }
    }
}
