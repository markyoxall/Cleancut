using System;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;

namespace CleanCut.WinApp.Infrastructure;

/// <summary>
/// Small helper to show a developer-friendly dialog that copies text to clipboard and uses DevEx-style layout.
/// This avoids using raw MessageBox throughout the codebase and gives users an easy way to copy error details.
/// </summary>
public static class DeveloperDialog
{
    /// <summary>
    /// Show a fatal error dialog with a copyable details box.
    /// </summary>
    public static void ShowFatal(string title, string message)
    {
        try
        {
            // Build a simple WinForms form to display the message and allow copying
            var form = new Form()
            {
                Text = title,
                Width = 600,
                Height = 320,
                StartPosition = FormStartPosition.CenterScreen
            };

            var label = new Label()
            {
                Text = message,
                AutoSize = false,
                Left = 12,
                Top = 12,
                Width = form.ClientSize.Width - 24,
                Height = 40
            };
            form.Controls.Add(label);

            var textBox = new TextBox()
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Left = 12,
                Top = label.Bottom + 8,
                Width = form.ClientSize.Width - 24,
                Height = form.ClientSize.Height - 110,
                Text = message
            };
            form.Controls.Add(textBox);

            var copyButton = new Button()
            {
                Text = "Copy",
                Left = 12,
                Top = textBox.Bottom + 8,
                Width = 80
            };
            copyButton.Click += (s, e) =>
            {
                try { Clipboard.SetText(textBox.Text); } catch { }
            };
            form.Controls.Add(copyButton);

            var okButton = new Button()
            {
                Text = "OK",
                Left = form.ClientSize.Width - 92,
                Top = textBox.Bottom + 8,
                Width = 80
            };
            okButton.Click += (s, e) => form.Close();
            form.Controls.Add(okButton);

            form.ShowDialog();
        }
        catch
        {
            // fallback to MessageBox if anything goes wrong
            try { MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error); } catch { }
        }
    }
}
