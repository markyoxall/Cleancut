namespace CleanCut.WinApp.MVP;

/// <summary>
/// Base form class that implements common view functionality
/// </summary>
public partial class BaseForm : Form, IView
{
    protected BaseForm()
    {
        InitializeComponent();
    }

    public virtual void ShowError(string message)
    {
        MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    public virtual void ShowInfo(string message)
    {
        MessageBox.Show(message, "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    public virtual void ShowSuccess(string message)
    {
        MessageBox.Show(message, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    public virtual bool ShowConfirmation(string message)
    {
        var result = MessageBox.Show(message, "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        return result == DialogResult.Yes;
    }

    public virtual void SetLoading(bool isLoading)
    {
        // Enable/disable the form and show cursor
        Enabled = !isLoading;
        Cursor = isLoading ? Cursors.WaitCursor : Cursors.Default;
        
        // Update UI on the main thread
        if (InvokeRequired)
        {
            Invoke(() => SetLoading(isLoading));
            return;
        }
    }

    private void InitializeComponent()
    {
        SuspendLayout();
        // 
        // BaseForm
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(800, 450);
        Name = "BaseForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "CleanCut Application";
        ResumeLayout(false);
    }
}