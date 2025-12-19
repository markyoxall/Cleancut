using System;
using System.Windows.Forms;
using System.Drawing;

namespace CleanCut.WinApp.MVP;

public partial class BaseForm
{
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
