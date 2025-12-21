using System.Windows.Forms;

namespace CleanCut.WinApp.Views.Customers;

public class PreferencesInputDialog : Form
{
    public string Layout => _layoutTextBox.Text;
    public string Theme => _themeTextBox.Text;

    private TextBox _layoutTextBox;
    private TextBox _themeTextBox;
    private Button _okButton;
    private Button _cancelButton;

    public PreferencesInputDialog(string initialLayout = "", string initialTheme = "")
    {
        Text = "Enter Preferences";
        Width = 350;
        Height = 180;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;

        var layoutLabel = new Label { Text = "Layout:", Left = 10, Top = 15, Width = 60 };
        _layoutTextBox = new TextBox { Left = 80, Top = 12, Width = 230, Text = initialLayout };
        var themeLabel = new Label { Text = "Theme:", Left = 10, Top = 50, Width = 60 };
        _themeTextBox = new TextBox { Left = 80, Top = 47, Width = 230, Text = initialTheme };

        _okButton = new Button { Text = "OK", Left = 155, Width = 75, Top = 90, DialogResult = DialogResult.OK };
        _cancelButton = new Button { Text = "Cancel", Left = 235, Width = 75, Top = 90, DialogResult = DialogResult.Cancel };

        _okButton.Click += (s, e) => { DialogResult = DialogResult.OK; Close(); };
        _cancelButton.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

        Controls.Add(layoutLabel);
        Controls.Add(_layoutTextBox);
        Controls.Add(themeLabel);
        Controls.Add(_themeTextBox);
        Controls.Add(_okButton);
        Controls.Add(_cancelButton);
        AcceptButton = _okButton;
        CancelButton = _cancelButton;
    }
}
