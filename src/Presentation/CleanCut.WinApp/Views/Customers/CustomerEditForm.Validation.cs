using System.Collections.Generic;
using System.Windows.Forms;
using DevExpress.XtraEditors.DXErrorProvider;

namespace CleanCut.WinApp.Views.Customers
{
    public partial class CustomerEditForm
    {
        private DXErrorProvider? _validationErrorProvider;

        private void EnsureErrorProvider()
        {
            if (_validationErrorProvider != null) return;
            _validationErrorProvider = new DXErrorProvider();
            // DXErrorProvider doesn't blink like WinForms ErrorProvider; no BlinkStyle
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
                    nameof(CustomerEditViewModel.FirstName) => _firstNameTextBox,
                    nameof(CustomerEditViewModel.LastName) => _lastNameTextBox,
                    nameof(CustomerEditViewModel.Email) => _emailTextBox,
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
            try
            {
                _validationErrorProvider?.ClearErrors();
            }
            catch { }
        }
    }
}
