using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace CleanCut.WinApp.Controls
{
    /// <summary>
    /// Lightweight Validation Summary control that provides a consistent appearance for validation messages.
    /// This is intentionally small and reusable across WinForms dialogs. It can be replaced by DevExpress' native
    /// ValidationSummaryControl if you want to adopt the vendor control later.
    /// </summary>
    public class ValidationSummaryControl : UserControl
    {
        private ListBox _listBox = new ListBox();

        public ValidationSummaryControl()
        {
            Initialize();
        }

        private void Initialize()
        {
            Height = 80;
            BorderStyle = BorderStyle.FixedSingle;
            _listBox = new ListBox
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.None
            };

            Controls.Add(_listBox);
            Visible = false;
        }

        public void SetMessages(IEnumerable<string> messages)
        {
            _listBox.Items.Clear();
            foreach (var m in messages)
                _listBox.Items.Add(m);

            Visible = _listBox.Items.Count > 0;
        }

        public void Clear()
        {
            _listBox.Items.Clear();
            Visible = false;
        }

        public void FocusFirst()
        {
            if (_listBox.Items.Count > 0)
                _listBox.Focus();
        }
    }
}
