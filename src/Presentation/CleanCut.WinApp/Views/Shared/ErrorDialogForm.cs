using System;
using System.Windows.Forms;

namespace CleanCut.WinApp.Views.Shared;

/// <summary>
/// Simple modal error dialog that shows an error message and optional details.
/// </summary>
public class ErrorDialogForm : Form
{
    private TextBox messageBox;
    private Button okButton;

    public ErrorDialogForm(string message, string? details = null)
    {
        Text = "Error";
        StartPosition = FormStartPosition.CenterParent;
        Width = 600;
        Height = 300;

        messageBox = new TextBox
        {
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            Dock = DockStyle.Fill,
            BackColor = SystemColors.Window,
            ForeColor = SystemColors.ControlText,
            Text = message + (string.IsNullOrWhiteSpace(details) ? string.Empty : ("\r\n\r\nDetails:\r\n" + details))
        };

        okButton = new Button
        {
            Text = "OK",
            Dock = DockStyle.Bottom,
            Height = 32
        };
        okButton.Click += (s, e) => Close();

        Controls.Add(messageBox);
        Controls.Add(okButton);
    }
}
