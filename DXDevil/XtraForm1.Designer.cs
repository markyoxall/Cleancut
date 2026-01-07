namespace DXDevil;

partial class XtraForm1
{
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        SuspendLayout();
        // 
        // XtraForm1
        // 
        AutoScaleDimensions = new SizeF(6F, 13F);
        AutoScaleMode = AutoScaleMode.Dpi;
        ClientSize = new Size(1104, 553);
        Name = "XtraForm1";
        Text = "XtraForm1";
        this.AccessibleName = "Per-form skinned window";
        this.AccessibleDescription = "A sample skinned form demonstrating per-form DevExpress skin";
        // Use a per-form skin different from the application-wide skin.
        // Disable the default look-and-feel and set a specific skin for this form.
        this.LookAndFeel.UseDefaultLookAndFeel = false;
        this.LookAndFeel.SkinName = "Office 2019 Colorful";
        ResumeLayout(false);
    }

    #endregion
}
