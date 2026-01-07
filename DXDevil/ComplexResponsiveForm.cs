// File: DXDevil\ComplexResponsiveForm.cs
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DevExpress.Data;
using DevExpress.Utils;
using DevExpress.XtraBars;
using DevExpress.XtraBars.Navigation;
using DevExpress.XtraCharts;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraEditors.ViewInfo;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraLayout;
using DevExpress.XtraTreeList;

namespace DXDevil
{
    /// <summary>
    /// Complex responsive DevExpress form demonstrating a dashboard-style layout with
    /// a local ribbon, KPI tiles (animated), grid, chart, tree and tabbed details.
    /// Uses typed mock data and a live-mode simulator to animate KPI changes.
    /// </summary>
    public partial class ComplexResponsiveForm : DevExpress.XtraEditors.XtraForm
    {
        // Simple typed DTO for mock orders used by the grid and KPI calculations
        private record MockOrder(int Id, string OrderNumber, string Customer, DateTime OrderDate, string Status, decimal Total, string Notes, Image? Picture);

        private LayoutControl _rootLayout;
        private SplitContainerControl _mainSplit;
        private AccordionControl _navAccordion;
        private DevExpress.XtraEditors.SplitContainerControl _rightSplit;
        private GridControl _grid;
        private GridView _gridView;
        private ChartControl _chart;
        private TreeList _tree;
        private MemoEdit _propertyMemo;
        private MemoEdit _detailMemo;
        private PictureEdit _picture;
        private SearchControl _search;
        private SimpleButton _refreshButton;
        private DevExpress.XtraBars.BarManager _barManager;
        private DevExpress.XtraBars.PopupMenu _popupMenu;
        private TabPane _tabPane;
        private TabNavigationPage _tabDetails;
        private TabNavigationPage _tabActivity;
        private TabNavigationPage _tabDocuments;
        private GridControl _docGrid;
        private GridView _docView;
        private ListBoxControl _activityList;
        private Dictionary<int, List<string>> _mockActivities = new();
        private Dictionary<int, List<object>> _mockDocuments = new();

        // Local ribbon
        private DevExpress.XtraBars.Ribbon.RibbonControl _localRibbon;
        private DevExpress.XtraBars.BarButtonItem _rbRefreshLocal;
        private DevExpress.XtraBars.BarButtonItem _rbExportLocal;
        private DevExpress.XtraBars.BarButtonItem _rbToggleViewLocal;
        private DevExpress.XtraBars.BarButtonItem _rbLiveToggle;

        // KPI tile visuals and animation
        private TileControl _tileControl;
        private TileGroup _tileGroup;
        private TileItem _tileOrders;
        private TileItem _tileRevenue;
        private TileItem _tileActive;
        private System.Windows.Forms.Timer _kpiTimer;
        private System.Windows.Forms.Timer _liveTimer;

        private int _displayOrders;
        private int _targetOrders;
        private double _displayRevenue;
        private double _targetRevenue;
        private int _displayActive;
        private int _targetActive;

        private List<MockOrder> _currentOrders = new();
        private readonly Random _rng = new();

        /// <summary>
        /// Creates a new responsive complex form with many DevExpress controls and mock data.
        /// </summary>
        public ComplexResponsiveForm()
        {
            AutoScaleMode = AutoScaleMode.Dpi;
            Text = "Complex Responsive DevExpress Form";
            StartPosition = FormStartPosition.CenterScreen;
            Width = 1200;
            Height = 800;
            Font = SystemFonts.MessageBoxFont;

            InitializeComponent();

            // Accessibility metadata
            AccessibleName = "Complex Responsive Dashboard";
            AccessibleDescription = "Main dashboard showing orders, charts and details with adaptive layout.";

            // Populate and start KPI animation
            PopulateSampleData();
            SetupKpiTimer();
            StartKpiAnimation();

            Resize += ComplexResponsiveForm_Resize;
            ApplyResponsiveRules();
        }

        /// <summary>
        /// Designer-generated UI initialization moved into the .Designer.cs file.
        /// The method body is implemented in ComplexResponsiveForm.Designer.cs
        /// to keep designer code separate from behavior.
        /// </summary>
        private void InitializeComponents()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Populate mock orders, chart, tree and details; set KPI targets.
        /// </summary>
        private void PopulateSampleData()
        {
            var customers = Enumerable.Range(1, 12).Select(i => $"Customer {i}").ToArray();
            var statuses = new[] { "Pending", "Confirmed", "Shipped", "Delivered", "Cancelled" };

            _currentOrders = Enumerable.Range(1, 50).Select(i => new MockOrder(
                i,
                $"ORD-{1000 + i}",
                customers[i % customers.Length],
                DateTime.Now.AddDays(-_rng.Next(0, 90)),
                statuses[_rng.Next(statuses.Length)],
                Math.Round((decimal) (50 + _rng.NextDouble() * 2000), 2),
                _rng.NextDouble() > 0.6 ? "Priority customer" : string.Empty,
                _rng.NextDouble() > 0.8 ? CreateSampleBitmap(i) : null
            )).ToList();

            var repoPic = new RepositoryItemPictureEdit();
            repoPic.Properties.SizeMode = PictureSizeMode.Zoom;
            _grid.RepositoryItems.Add(repoPic);

            _grid.DataSource = _currentOrders;
            _gridView.PopulateColumns();

            var idCol = _gridView.Columns.ColumnByFieldName("Id"); if (idCol != null) idCol.Visible = false;
            var picCol = _gridView.Columns.ColumnByFieldName("Picture"); if (picCol != null) { picCol.Caption = ""; picCol.ColumnEdit = repoPic; picCol.OptionsColumn.AllowEdit = false; picCol.Width = 80; }
            var totalCol = _gridView.Columns.ColumnByFieldName("Total"); if (totalCol != null) { totalCol.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric; totalCol.DisplayFormat.FormatString = "c"; totalCol.Width = 120; }
            var dateCol = _gridView.Columns.ColumnByFieldName("OrderDate"); if (dateCol != null) { dateCol.DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime; dateCol.DisplayFormat.FormatString = "g"; dateCol.Width = 160; }
            var statusCol = _gridView.Columns.ColumnByFieldName("Status"); if (statusCol != null) statusCol.Width = 120;

            // chart
            var series = new Series("Sales", ViewType.Line);
            var months = Enumerable.Range(0, 12).Select(i => DateTime.Now.AddMonths(-i)).Reverse().ToArray();
            foreach (var m in months) series.Points.Add(new SeriesPoint(m.ToString("MMM yy"), new double[] { Math.Round(800 + (_rng.NextDouble() * 2400), 2) }));
            _chart.Series.Clear(); _chart.Series.Add(series); ((LineSeriesView) series.View).MarkerVisibility = DefaultBoolean.True;

            // tree
            _tree.Nodes.Clear();
            for (int i = 0; i < 5; i++)
            {
                var root = _tree.AppendNode(new object[] { $"Category {i + 1}" }, null);
                for (int j = 0; j < 3; j++) _tree.AppendNode(new object[] { $"Sub {i + 1}.{j + 1}" }, root);
            }

            // details mock
            _mockActivities.Clear(); _mockDocuments.Clear();
            foreach (var o in _currentOrders)
            {
                _mockActivities[o.Id] = Enumerable.Range(1, 5).Select(i => $"Activity {i} for order {o.Id}").ToList();
                _mockDocuments[o.Id] = Enumerable.Range(1, 3).Select(i => (object) new { Name = $"Doc_{o.Id}_{i}.pdf", Size = $"{_rng.Next(10, 200)} KB" }).ToList();
            }

            _docGrid.DataSource = _mockDocuments.Values.FirstOrDefault() ?? new List<object>();

            // KPI targets
            _targetOrders = _currentOrders.Count;
            _targetRevenue = (double) _currentOrders.Sum(x => x.Total);
            _targetActive = _currentOrders.Count(x => x.Status == "Confirmed");

            // ensure tile items are present and initialized
            EnsureTilesPresent();
        }

        private static Bitmap CreateSampleBitmap(int i)
        {
            var bmp = new Bitmap(64, 48);
            using var g = Graphics.FromImage(bmp);
            g.Clear(Color.FromArgb(150 + (i * 3) % 100, (i * 7) % 200, (i * 13) % 200));
            g.DrawString(i.ToString(), SystemFonts.DefaultFont, Brushes.White, new PointF(6, 6));
            return bmp;
        }

        private void EnsureTilesPresent()
        {
            try
            {
                if (_tileControl == null) return;
                if (_tileGroup == null)
                {
                    _tileOrders = new TileItem();
                    _tileOrders.Elements.Add(new TileItemElement { Text = "Orders\n0", TextAlignment = TileItemContentAlignment.MiddleCenter });
                    _tileRevenue = new TileItem();
                    _tileRevenue.Elements.Add(new TileItemElement { Text = "Revenue\n$0", TextAlignment = TileItemContentAlignment.MiddleCenter });
                    _tileActive = new TileItem();
                    _tileActive.Elements.Add(new TileItemElement { Text = "Active\n0", TextAlignment = TileItemContentAlignment.MiddleCenter });
                    _tileGroup = new TileGroup();
            _tileGroup.Items.Add(_tileOrders);
            _tileGroup.Items.Add(_tileRevenue);
            _tileGroup.Items.Add(_tileActive);
                    _tileControl.Groups.Add(_tileGroup);
                }
            }
            catch { }
        }

        private void SetupKpiTimer()
        {
            if (_kpiTimer != null) { _kpiTimer.Stop(); _kpiTimer.Dispose(); }
            _kpiTimer = new System.Windows.Forms.Timer { Interval = 40 };
            _kpiTimer.Tick += (s, e) => AnimateKpisStep();
        }

        private void StartKpiAnimation()
        {
            _displayOrders = 0; _displayRevenue = 0; _displayActive = 0;
            _kpiTimer?.Start();
        }

        private void AnimateKpisStep()
        {
            const double ease = 0.16;
            _displayOrders = (int) Math.Round(_displayOrders + (_targetOrders - _displayOrders) * ease);
            _displayRevenue = _displayRevenue + (_targetRevenue - _displayRevenue) * ease;
            _displayActive = (int) Math.Round(_displayActive + (_targetActive - _displayActive) * ease);

            try
            {
                if (_tileOrders != null) _tileOrders.Elements[0].Text = $"Orders\n{_displayOrders:N0}";
                if (_tileRevenue != null) _tileRevenue.Elements[0].Text = $"Revenue\n{_displayRevenue:C0}";
                if (_tileActive != null) _tileActive.Elements[0].Text = $"Active\n{_displayActive:N0}";
            }
            catch { }

            if (Math.Abs(_displayRevenue - _targetRevenue) < 1 && _displayOrders == _targetOrders && _displayActive == _targetActive)
            {
                _kpiTimer?.Stop();
            }
        }

        private void ToggleLiveMode()
        {
            if (_liveTimer == null)
            {
                _liveTimer = new System.Windows.Forms.Timer { Interval = 3000 };
                _liveTimer.Tick += (s, e) => SimulateLiveChanges();
                _liveTimer.Start();
                _rbLiveToggle.Caption = "Live (On)";
                return;
            }

            if (_liveTimer.Enabled) { _liveTimer.Stop(); _rbLiveToggle.Caption = "Live"; }
            else { _liveTimer.Start(); _rbLiveToggle.Caption = "Live (On)"; }
        }

        private void SimulateLiveChanges()
        {
            if (_currentOrders == null || _currentOrders.Count == 0) return;
            int changes = Math.Max(1, _currentOrders.Count / 20);
            for (int i = 0; i < changes; i++)
            {
                var idx = _rng.Next(_currentOrders.Count);
                var o = _currentOrders[idx];
                var newTotal = Math.Round(o.Total * (decimal) (0.9 + _rng.NextDouble() * 0.4), 2);
                var statuses = new[] { "Pending", "Confirmed", "Shipped", "Delivered", "Cancelled" };
                var newStatus = statuses[_rng.Next(statuses.Length)];
                _currentOrders[idx] = new MockOrder(o.Id, o.OrderNumber, o.Customer, o.OrderDate, newStatus, newTotal, o.Notes, o.Picture);
                if (_mockActivities.TryGetValue(o.Id, out var acts)) acts.Add($"Auto update at {DateTime.Now:T}: total {newTotal:C}");
            }

            // refresh grid and recompute targets
            _grid.DataSource = null;
            _grid.DataSource = _currentOrders;
            _gridView.RefreshData();

            _targetOrders = _currentOrders.Count;
            _targetRevenue = (double) _currentOrders.Sum(x => x.Total);
            _targetActive = _currentOrders.Count(x => x.Status == "Confirmed");

            StartKpiAnimation();
        }

        private void OnGridFocusedRowChanged()
        {
            var row = _gridView.GetFocusedRow();
            if (row is MockOrder ord)
            {
                _propertyMemo.Text = $"Order: {ord.OrderNumber}\r\nCustomer: {ord.Customer}\r\nTotal: {ord.Total:C}\r\nStatus: {ord.Status}";
                _activityList.Items.Clear();
                if (_mockActivities.TryGetValue(ord.Id, out var acts)) foreach (var a in acts) _activityList.Items.Add(a);
                if (_mockDocuments.TryGetValue(ord.Id, out var docs)) { _docGrid.DataSource = docs; _docView.PopulateColumns(); }
            }
            else
            {
                _propertyMemo.Text = string.Empty;
                _activityList.Items.Clear();
            }
        }

        private void UpdateTreeSelection()
        {
            var node = _tree.FocusedNode;
            if (node != null) _propertyMemo.Text = $"Node: {node.GetDisplayText(0)}";
        }

        private void RefreshData()
        {
            PopulateSampleData();
            StartKpiAnimation();
        }

        private void ToggleGridViewMode()
        {
            // placeholder for toggling views; keep UI responsive
            XtraMessageBox.Show(this, "Toggle view pressed (demo)");
        }

        private void ApplyResponsiveRules()
        {
            var w = ClientSize.Width;
            if (w < 900)
            {
                _mainSplit.SplitterPosition = 140;
                _rightSplit.SplitterPosition = 60;
                _tileControl.Visible = false;
            }
            else if (w < 1200)
            {
                _mainSplit.SplitterPosition = 220;
                _rightSplit.SplitterPosition = 80;
                _tileControl.Visible = true;
            }
            else
            {
                _mainSplit.SplitterPosition = 260;
                _rightSplit.SplitterPosition = 120;
                _tileControl.Visible = true;
            }

            _rootLayout.Refresh();
        }

        private void ComplexResponsiveForm_Resize(object? sender, EventArgs e) => ApplyResponsiveRules();
    }
}
