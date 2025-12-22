using System;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraEditors;

namespace CleanCut.WinApp.Views.Shared
{
    /// <summary>
    /// DevExpress XtraForm dialog for displaying errors with details and simple actions.
    /// </summary>
    public class DevExpressErrorDialogForm : XtraForm
    {
        private MemoEdit _messageBox;
        private SimpleButton _okButton;
        private SimpleButton _cancelButton;

        public DevExpressErrorDialogForm(string message, string? details = null)
        {
            Text = "Error";
            StartPosition = FormStartPosition.CenterParent;
            Width = 700;
            Height = 360;

            _messageBox = new MemoEdit
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                Text = message + (string.IsNullOrWhiteSpace(details) ? string.Empty : "\r\n\r\nDetails:\r\n" + details),
                Properties = { ScrollBars = ScrollBars.Vertical }
            };

            _okButton = new SimpleButton { Text = "Continue", Dock = DockStyle.Right, Width = 120 };
            _cancelButton = new SimpleButton { Text = "Exit", Dock = DockStyle.Right, Width = 120 };

            _okButton.Click += (s, e) => { DialogResult = DialogResult.OK; Close(); };
            _cancelButton.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            var panel = new Panel { Dock = DockStyle.Bottom, Height = 40 };
            panel.Controls.Add(_cancelButton);
            panel.Controls.Add(_okButton);

            Controls.Add(_messageBox);
            Controls.Add(panel);
        }
    }
}
