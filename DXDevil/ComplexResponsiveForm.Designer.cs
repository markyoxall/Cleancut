// Designer file for ComplexResponsiveForm - extracted UI initialization
using System;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.Utils;
using DevExpress.XtraBars;
using DevExpress.XtraBars.Navigation;
using DevExpress.XtraCharts;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraLayout;
using DevExpress.XtraTreeList;

namespace DXDevil
{
    partial class ComplexResponsiveForm
    {
        /// <summary>
        /// Designer-generated initialization of controls (moved from code-behind so the Visual Studio designer
        /// can display the full layout). This method instantiates all UI controls and adds them to the form.
        /// </summary>
        private void InitializeComponent()
        {
            // Initialize controls used by the form. Keep this in the designer file so the Visual Studio
            // WinForms designer can render the layout.
            _localRibbon = new DevExpress.XtraBars.Ribbon.RibbonControl { Dock = DockStyle.Top };
            _rbRefreshLocal = new DevExpress.XtraBars.BarButtonItem { Caption = "Refresh" };
            _rbExportLocal = new DevExpress.XtraBars.BarButtonItem { Caption = "Export" };
            _rbToggleViewLocal = new DevExpress.XtraBars.BarButtonItem { Caption = "Toggle View" };
            _rbLiveToggle = new DevExpress.XtraBars.BarButtonItem { Caption = "Live" };

            _rootLayout = new LayoutControl { Dock = DockStyle.Fill };
            _mainSplit = new SplitContainerControl { Dock = DockStyle.Fill, Horizontal = false };
            _navAccordion = new AccordionControl { Dock = DockStyle.Fill };
            _rightSplit = new DevExpress.XtraEditors.SplitContainerControl { Dock = DockStyle.Fill, Horizontal = true };

            _tileControl = new TileControl { Dock = DockStyle.Left, Width = 420 };
            _tileGroup = new TileGroup();
            _tileOrders = new TileItem();
            _tileRevenue = new TileItem();
            _tileActive = new TileItem();
            _tileOrders.Elements.Add(new TileItemElement { Text = "Orders\n0", TextAlignment = TileItemContentAlignment.MiddleCenter });
            _tileRevenue.Elements.Add(new TileItemElement { Text = "Revenue\n$0", TextAlignment = TileItemContentAlignment.MiddleCenter });
            _tileActive.Elements.Add(new TileItemElement { Text = "Active\n0", TextAlignment = TileItemContentAlignment.MiddleCenter });
            _tileOrders.AppearanceItem.Normal.BackColor = Color.FromArgb(0x2D, 0x89, 0xFF);
            _tileRevenue.AppearanceItem.Normal.BackColor = Color.FromArgb(0x28, 0xA7, 0x45);
            _tileActive.AppearanceItem.Normal.BackColor = Color.FromArgb(0xFF, 0xA5, 0x00);
            _tileGroup.Items.Add(_tileOrders);
            _tileGroup.Items.Add(_tileRevenue);
            _tileGroup.Items.Add(_tileActive);
            _tileControl.Groups.Add(_tileGroup);

            _search = new SearchControl { Properties = { NullValuePrompt = "Search orders, customers..." }, Dock = DockStyle.Fill };
            _refreshButton = new SimpleButton { Text = "Refresh", Dock = DockStyle.Right };

            _chart = new ChartControl { Dock = DockStyle.Fill, PaletteName = "Office" };

            _grid = new GridControl { Dock = DockStyle.Fill };
            _gridView = new GridView(_grid) { Name = "gridView" };
            _grid.MainView = _gridView;
            _grid.ViewCollection.Add(_gridView);

            _tree = new TreeList { Dock = DockStyle.Fill };
            _propertyMemo = new MemoEdit { Dock = DockStyle.Fill, ReadOnly = true };
            _detailMemo = new MemoEdit { Dock = DockStyle.Fill, ReadOnly = true };
            _picture = new PictureEdit { Dock = DockStyle.Fill, Properties = { SizeMode = PictureSizeMode.Squeeze } };

            _tabPane = new TabPane { Dock = DockStyle.Fill };
            _tabDetails = new TabNavigationPage { Caption = "Details" };
            _tabActivity = new TabNavigationPage { Caption = "Activity" };
            _tabDocuments = new TabNavigationPage { Caption = "Documents" };

            _activityList = new ListBoxControl { Dock = DockStyle.Fill };
            _docGrid = new GridControl { Dock = DockStyle.Fill };
            _docView = new GridView(_docGrid);
            _docGrid.MainView = _docView;
            _docGrid.ViewCollection.Add(_docView);

            // Build container hierarchy so the designer shows the full layout
            // Header panel (search + refresh + tiles)
            var headerPanel = new Panel { Dock = DockStyle.Fill };
            headerPanel.Controls.Add(_search);
            headerPanel.Controls.Add(_refreshButton);
            headerPanel.Controls.Add(_tileControl);

            var headerSplit = new DevExpress.XtraEditors.SplitContainerControl { Dock = DockStyle.Fill, Horizontal = false };
            headerSplit.SplitterPosition = 420 + 20;
            headerSplit.Panel1.Controls.Add(headerPanel);
            headerSplit.Panel2.Controls.Add(_chart);

            // Bottom split: grid above, details below
            var bottomSplit = new DevExpress.XtraEditors.SplitContainerControl { Dock = DockStyle.Fill, Horizontal = false };
            bottomSplit.SplitterPosition = 360;
            bottomSplit.Panel1.Controls.Add(_grid);

            var detailsPanel = new Panel { Dock = DockStyle.Fill };
            _tree.Dock = DockStyle.Left;
            _tree.Width = 180;
            _tabPane.Dock = DockStyle.Fill;
            detailsPanel.Controls.Add(_tabPane);
            detailsPanel.Controls.Add(_tree);
            bottomSplit.Panel2.Controls.Add(detailsPanel);

            // Right area: header + bottom
            _rightSplit.Panel1.Controls.Add(headerSplit);
            _rightSplit.Panel2.Controls.Add(bottomSplit);

            // Left navigation -> main split
            _mainSplit.Panel1.Controls.Add(_navAccordion);
            _mainSplit.Panel2.Controls.Add(_rightSplit);

            // Root layout contains the main split
            _rootLayout.Root = new LayoutControlGroup();
            _rootLayout.Controls.Add(_mainSplit);
            _rootLayout.Root.AddItem(new LayoutControlItem { Control = _mainSplit, TextVisible = false });

            // Add root layout and ribbon to the form
            this.SuspendLayout();
            this.ClientSize = new Size(1000, 700);
            this.Controls.Add(_rootLayout);
            this.Controls.Add(_localRibbon);
            this.Name = "ComplexResponsiveForm";
            this.ResumeLayout(false);
        }
    }
}
