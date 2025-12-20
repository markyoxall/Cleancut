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
    private ToolStripMenuItem userManagementMenuItem = null!;
    private ToolStripMenuItem productManagementMenuItem = null!;
    private ToolStripMenuItem countryManagementToolStripMenuItem = null!;
    private ToolStripMenuItem orderManagementMenuItem = null!;

    private void InitializeComponent()
    {
        menuStrip = new MenuStrip();
        fileMenu = new ToolStripMenuItem();
        exitMenuItem = new ToolStripMenuItem();
        managementMenu = new ToolStripMenuItem();
        userManagementMenuItem = new ToolStripMenuItem();
        productManagementMenuItem = new ToolStripMenuItem();
        countryManagementToolStripMenuItem = new ToolStripMenuItem();
        orderManagementMenuItem = new ToolStripMenuItem();
        menuStrip.SuspendLayout();
        SuspendLayout();
        // 
        // menuStrip
        // 
        menuStrip.Items.AddRange(new ToolStripItem[] { fileMenu, managementMenu });
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
        // 
        // managementMenu
        // 
        managementMenu.DropDownItems.AddRange(new ToolStripItem[] { userManagementMenuItem, productManagementMenuItem, countryManagementToolStripMenuItem, orderManagementMenuItem });
        managementMenu.Name = "managementMenu";
        managementMenu.Size = new Size(62, 20);
        managementMenu.Text = "Manage";
        // 
        // userManagementMenuItem
        // 
        userManagementMenuItem.Name = "userManagementMenuItem";
        userManagementMenuItem.Size = new Size(200, 22);
        userManagementMenuItem.Text = "Customer Management";
        userManagementMenuItem.Click += OnCustomerManagementClicked;
        // 
        // productManagementMenuItem
        // 
        productManagementMenuItem.Name = "productManagementMenuItem";
        productManagementMenuItem.Size = new Size(200, 22);
        productManagementMenuItem.Text = "Product Management";
        productManagementMenuItem.Click += OnProductManagementClicked;
        // 
        // countryManagementToolStripMenuItem
        // 
        countryManagementToolStripMenuItem.Name = "countryManagementToolStripMenuItem";
        countryManagementToolStripMenuItem.Size = new Size(200, 22);
        countryManagementToolStripMenuItem.Text = "Country Management";
        countryManagementToolStripMenuItem.Click += OnCountryManagementClicked;
        // 
        // orderManagementMenuItem
        // 
        orderManagementMenuItem.Name = "orderManagementMenuItem";
        orderManagementMenuItem.Size = new Size(200, 22);
        orderManagementMenuItem.Text = "Order Management";
        orderManagementMenuItem.Click += OnOrderManagementClicked;
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
