using System;
using System.Windows.Forms;
using System.Drawing;

namespace CleanCut.WinApp;

public partial class MainForm
{
    private MenuStrip menuStrip = null!;
    private ToolStripMenuItem fileMenu = null!;
    private ToolStripMenuItem exitMenuItem = null!;
    private ToolStripMenuItem managementMenu = null!;
    private ToolStripMenuItem adminMenu = null!;
    private ToolStripMenuItem manageModulesMenuItem = null!;

    private void InitializeComponent()
    {
        menuStrip = new MenuStrip();
        fileMenu = new ToolStripMenuItem();
        exitMenuItem = new ToolStripMenuItem();
        managementMenu = new ToolStripMenuItem();
        adminMenu = new ToolStripMenuItem();
        manageModulesMenuItem = new ToolStripMenuItem();
        menuStrip.SuspendLayout();
        SuspendLayout();
        // 
        // menuStrip
        // 
        menuStrip.Items.AddRange(new ToolStripItem[] { fileMenu, managementMenu, adminMenu });
        menuStrip.Location = new Point(0, 0);
        menuStrip.Name = "menuStrip";
        menuStrip.Size = new Size(1024, 24);
        menuStrip.TabIndex = 0;
        menuStrip.Text = "menuStrip";
        // 
        // fileMenu
        // 
        fileMenu.DropDownItems.AddRange(new ToolStripItem[] { exitMenuItem });
        fileMenu.Name = "fileMenu";
        fileMenu.Size = new Size(37, 20);
        fileMenu.Text = "File";
        // 
        // exitMenuItem
        // 
        exitMenuItem.Name = "exitMenuItem";
        exitMenuItem.Size = new Size(92, 22);
        exitMenuItem.Text = "Exit";
        exitMenuItem.Click += OnExitClicked;
        // 
        // managementMenu
        // 
        // Placeholder - items added at runtime from configuration
        managementMenu.Name = "managementMenu";
        managementMenu.Size = new Size(62, 20);
        managementMenu.Text = "Manage";
        // 
        // adminMenu
        // 
        adminMenu.DropDownItems.AddRange(new ToolStripItem[] { manageModulesMenuItem });
        adminMenu.Name = "adminMenu";
        adminMenu.Size = new Size(55, 20);
        adminMenu.Text = "Admin";
        // 
        // manageModulesMenuItem
        // 
        manageModulesMenuItem.Name = "manageModulesMenuItem";
        manageModulesMenuItem.Size = new Size(200, 22);
        manageModulesMenuItem.Text = "Manage Modules...";
        manageModulesMenuItem.Click += OnManageModulesClicked;
        // 
        // MainForm
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        ClientSize = new Size(1024, 768);
        Controls.Add(menuStrip);
        IsMdiContainer = true;
        MainMenuStrip = menuStrip;
        Name = "MainForm";
        Text = "CleanCut Desktop Application";
        WindowState = FormWindowState.Maximized;
        menuStrip.ResumeLayout(false);
        menuStrip.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }
}
