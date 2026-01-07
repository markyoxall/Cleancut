// Designer partial for MainMenuForm
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraBars;
using DevExpress.XtraBars.Ribbon;

namespace DXDevil
{
    partial class MainMenuForm
    {
        private RibbonControl ribbon;
        private BarButtonItem btnOpenForm1;
        private BarButtonItem btnOpenXtra;
        private BarButtonItem btnOpenComplex;
        private RibbonPage page;
        private RibbonPageGroup group;

        /// <summary>
        /// Designer-generated initialization. Keeps UI creation in the partial class for designer support.
        /// </summary>
        private void InitializeComponent()
        {
            ribbon = new RibbonControl();
            btnOpenForm1 = new BarButtonItem();
            btnOpenXtra = new BarButtonItem();
            btnOpenComplex = new BarButtonItem();
            page = new RibbonPage();
            group = new RibbonPageGroup();

            ((System.ComponentModel.ISupportInitialize)ribbon).BeginInit();
            SuspendLayout();

            // ribbon
            ribbon.ExpandCollapseItem.Id = 0;

            btnOpenComplex.Caption = "Open Complex Form";
            btnOpenComplex.Id = 3;
            btnOpenComplex.Name = "btnOpenComplex";
            btnOpenComplex.ItemClick += OpenComplex_ItemClick;

            ribbon.Items.AddRange(new BarItem[] { ribbon.ExpandCollapseItem, btnOpenForm1, btnOpenXtra, btnOpenComplex });
            ribbon.Location = new Point(0, 0);
            ribbon.MaxItemId = 4;
            ribbon.Name = "ribbon";
            ribbon.Pages.AddRange(new RibbonPage[] { page });
            ribbon.Size = new Size(998, 158);

            btnOpenForm1.Caption = "Open Form1 (Ribbon)";
            btnOpenForm1.Id = 1;
            btnOpenForm1.Name = "btnOpenForm1";
            btnOpenForm1.ItemClick += OpenForm1_ItemClick;

            btnOpenXtra.Caption = "Open XtraForm1 (Per-form skin)";
            btnOpenXtra.Id = 2;
            btnOpenXtra.Name = "btnOpenXtra";
            btnOpenXtra.ItemClick += OpenXtra_ItemClick;

            page.Groups.AddRange(new RibbonPageGroup[] { group });
            page.Name = "page";
            page.Text = "Home";

            group.ItemLinks.Add(btnOpenForm1);
            group.ItemLinks.Add(btnOpenXtra);
            group.ItemLinks.Add(btnOpenComplex);
            group.Name = "group";
            group.Text = "Windows";

            // MainMenuForm
            ClientSize = new Size(998, 699);
            Controls.Add(ribbon);
            IsMdiContainer = true;
            Name = "MainMenuForm";
            Ribbon = ribbon;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Main Menu";

            ((System.ComponentModel.ISupportInitialize)ribbon).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }
    }
}
