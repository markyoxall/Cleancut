namespace DXDevil;

partial class Form1
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
        gridControl = new DevExpress.XtraGrid.GridControl();
        gridView = new DevExpress.XtraGrid.Views.Grid.GridView();
        cardView = new DevExpress.XtraGrid.Views.Card.CardView();
        ribbonControl = new DevExpress.XtraBars.Ribbon.RibbonControl();
        skinRibbonGalleryBarItem = new DevExpress.XtraBars.SkinRibbonGalleryBarItem();
        ribbonPage = new DevExpress.XtraBars.Ribbon.RibbonPage();
        rpgSkins = new DevExpress.XtraBars.Ribbon.RibbonPageGroup();
        skinDropDownButtonItem = new DevExpress.XtraBars.SkinDropDownButtonItem();
        skinPaletteRibbonGalleryBarItem = new DevExpress.XtraBars.SkinPaletteRibbonGalleryBarItem();
        bbiToggleView = new DevExpress.XtraBars.BarButtonItem();
        ((System.ComponentModel.ISupportInitialize) (gridControl)).BeginInit();
        ((System.ComponentModel.ISupportInitialize) (gridView)).BeginInit();
        ((System.ComponentModel.ISupportInitialize) (cardView)).BeginInit();
        repositoryPictureEdit = new DevExpress.XtraEditors.Repository.RepositoryItemPictureEdit();
        ((System.ComponentModel.ISupportInitialize) (repositoryPictureEdit)).BeginInit();
        ((System.ComponentModel.ISupportInitialize) (ribbonControl)).BeginInit();
        SuspendLayout();
        // 
        // gridControl
        // 
        gridControl.Dock = System.Windows.Forms.DockStyle.Fill;
        gridControl.AccessibleName = "Orders Grid";
        gridControl.AccessibleDescription = "Displays orders and their details";
        gridControl.Location = new System.Drawing.Point(0, 143);
        gridControl.MainView = gridView;
        gridControl.Name = "gridControl";
        gridControl.Size = new System.Drawing.Size(905, 348);
        gridControl.TabIndex = 0;
        gridControl.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
        gridView,
        cardView});
        // repository items
        gridControl.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[] {
        repositoryPictureEdit});
        // 
        // gridView
        // 
        gridView.GridControl = gridControl;
        gridView.Name = "gridView";
        // 
        // cardView
        // 
        cardView.GridControl = gridControl;
        cardView.Name = "cardView";
        // 
        // ribbonControl
        // 
        ribbonControl.ExpandCollapseItem.Id = 0;
        // instantiate items that will be added to the ribbon
        barStaticViewMode = new DevExpress.XtraBars.BarStaticItem();
        rpgView = new DevExpress.XtraBars.Ribbon.RibbonPageGroup();
        ribbonControl.Items.AddRange(new DevExpress.XtraBars.BarItem[] {
        ribbonControl.ExpandCollapseItem,
        skinRibbonGalleryBarItem,
        ribbonControl.SearchEditItem,
        skinDropDownButtonItem,
        skinPaletteRibbonGalleryBarItem,
        bbiToggleView,
        barStaticViewMode});
        ribbonControl.Location = new System.Drawing.Point(0, 0);
        ribbonControl.MaxItemId = 5;
        // ensure MaxItemId covers added items
        ribbonControl.MaxItemId = 7;
        ribbonControl.Name = "ribbonControl";
        ribbonControl.AccessibleName = "Main toolbar";
        ribbonControl.AccessibleDescription = "Application toolbar containing skin and view controls";
        ribbonControl.Pages.AddRange(new DevExpress.XtraBars.Ribbon.RibbonPage[] {
        ribbonPage});
        ribbonControl.Size = new System.Drawing.Size(905, 147);
        // 
        // skinRibbonGalleryBarItem
        // 
        skinRibbonGalleryBarItem.Caption = "skinRibbonGalleryBarItem";
        skinRibbonGalleryBarItem.Id = 1;
        skinRibbonGalleryBarItem.Name = "skinRibbonGalleryBarItem";
        // 
        // ribbonPage
        // 
        ribbonPage.Groups.AddRange(new DevExpress.XtraBars.Ribbon.RibbonPageGroup[] {
        rpgSkins,
        rpgView});
        ribbonPage.Name = "ribbonPage";
        ribbonPage.Text = "ribbonPage";
        // 
        // rpgSkins
        // 
        rpgSkins.ItemLinks.Add(skinDropDownButtonItem);
        rpgSkins.ItemLinks.Add(skinPaletteRibbonGalleryBarItem);
        rpgSkins.Name = "rpgSkins";
        rpgSkins.Text = "Skins";
        // 
        // rpgView - configure the already-instantiated group
        // 
        rpgView.ItemLinks.Add(bbiToggleView);
        rpgView.ItemLinks.Add(barStaticViewMode);
        rpgView.Name = "rpgView";
        rpgView.Text = "View";
        // 
        // bbiToggleView
        // 
        bbiToggleView.Caption = "Toggle View";
        bbiToggleView.Id = 4;
        bbiToggleView.Name = "bbiToggleView";
        bbiToggleView.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.BbiToggleView_ItemClick);
        // 
        // barStaticViewMode
        // 
        barStaticViewMode.Id = 5;
        barStaticViewMode.Name = "barStaticViewMode";
        barStaticViewMode.Caption = "View: Grid";
        // 
        // skinDropDownButtonItem
        // 
        skinDropDownButtonItem.Id = 2;
        skinDropDownButtonItem.Name = "skinDropDownButtonItem";
        // 
        // skinPaletteRibbonGalleryBarItem
        // 
        skinPaletteRibbonGalleryBarItem.Caption = "skinPaletteRibbonGalleryBarItem";
        skinPaletteRibbonGalleryBarItem.Id = 3;
        skinPaletteRibbonGalleryBarItem.Name = "skinPaletteRibbonGalleryBarItem";
        // 
        // Form
        // 
        AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        // Use system font to respect user preferences and DPI scaling
        this.Font = System.Drawing.SystemFonts.MessageBoxFont;
        this.AccessibleName = "Orders";
        this.AccessibleDescription = "Order list and management";
        ClientSize = new System.Drawing.Size(905, 491);
        Controls.Add(gridControl);
        Controls.Add(ribbonControl);
        Name = "Form";
        Ribbon = ribbonControl;
        Text = "Form";
        ((System.ComponentModel.ISupportInitialize) (gridControl)).EndInit();
        ((System.ComponentModel.ISupportInitialize) (gridView)).EndInit();
        ((System.ComponentModel.ISupportInitialize) (cardView)).EndInit();
        ((System.ComponentModel.ISupportInitialize) (repositoryPictureEdit)).EndInit();
        ((System.ComponentModel.ISupportInitialize) (ribbonControl)).EndInit();
        ResumeLayout(false);
        PerformLayout();

    }

    #endregion

    private DevExpress.XtraGrid.GridControl gridControl;
    private DevExpress.XtraGrid.Views.Grid.GridView gridView;
    private DevExpress.XtraGrid.Views.Card.CardView cardView;
    private DevExpress.XtraEditors.Repository.RepositoryItemPictureEdit repositoryPictureEdit;
    private DevExpress.XtraBars.Ribbon.RibbonControl ribbonControl;
    private DevExpress.XtraBars.Ribbon.RibbonPage ribbonPage;
    private DevExpress.XtraBars.Ribbon.RibbonPageGroup rpgSkins;
    private DevExpress.XtraBars.SkinRibbonGalleryBarItem skinRibbonGalleryBarItem;
    private DevExpress.XtraBars.SkinDropDownButtonItem skinDropDownButtonItem;
    private DevExpress.XtraBars.SkinPaletteRibbonGalleryBarItem skinPaletteRibbonGalleryBarItem;
    private DevExpress.XtraBars.BarButtonItem bbiToggleView;
    private DevExpress.XtraBars.Ribbon.RibbonPageGroup rpgView;
    private DevExpress.XtraBars.BarStaticItem barStaticViewMode;
}
