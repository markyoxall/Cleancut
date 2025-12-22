using System.Collections.Generic;
using System.Windows.Forms;
using DevExpress.XtraEditors.DXErrorProvider;

namespace CleanCut.WinApp.Views.Products
{
    public partial class ProductEditForm
    {
        private DXErrorProvider? _validationErrorProvider;

        private void EnsureErrorProvider()
        {
            if (_validationErrorProvider != null) return;
            _validationErrorProvider = new DXErrorProvider();
        }

        public void ShowValidationErrors(Dictionary<string, string> errors)
        {
            if (errors == null) return;
            EnsureErrorProvider();
            ClearValidationErrors();

            foreach (var kv in errors)
            {
                var property = kv.Key;
                var message = kv.Value;

                Control? ctl = property switch
                {
                    nameof(ProductEditViewModel.Name) => _nameTextBox,
                    nameof(ProductEditViewModel.Description) => _descriptionTextBox,
                    nameof(ProductEditViewModel.Price) => _priceNumericUpDown,
                    nameof(ProductEditViewModel.UserId) => _userComboBox,
                    _ => null
                };

                if (ctl != null)
                {
                    _validationErrorProvider.SetError(ctl, message);
                }
            }
        }

        public void ClearValidationErrors()
        {
            try { _validationErrorProvider?.ClearErrors(); } catch { }
        }
    }
}
