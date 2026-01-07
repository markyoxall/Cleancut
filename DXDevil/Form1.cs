using DevExpress.XtraBars.Ribbon;
using DXDevil.Data;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace DXDevil;

public partial class Form1 : RibbonForm
{
    private readonly IOrderRepository _orderRepository;

    /// <summary>
    /// Parameterless constructor used by the application entry point.
    /// Creates a default SqlOrderRepository instance.
    /// </summary>
    public Form1() : this(new SqlOrderRepository()) { }

    /// <summary>
    /// Creates a new instance of <see cref="Form1"/>.
    /// </summary>
    public Form1(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        InitializeComponent();
        ConfigureGrid();
        Shown += async (s, e) => await LoadAndBindOrdersAsync();
    }

    private void ConfigureGrid()
    {
   
        var view = gridView;
        view.Columns.Clear();

        var properties = typeof(OrderListGridItem).GetProperties();
        foreach (var prop in properties)
        {
            var column = view.Columns.AddVisible(prop.Name, SplitCamelCase(prop.Name));
            column.Visible = true;
            if (prop.PropertyType == typeof(DateTime) || prop.PropertyType == typeof(DateTime?))
            {
                column.DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
                column.DisplayFormat.FormatString = "g";
            }
            else if (prop.PropertyType == typeof(decimal) || prop.PropertyType == typeof(double) || prop.PropertyType == typeof(float))
            {
                column.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
                column.DisplayFormat.FormatString = "c";
            }
            column.OptionsColumn.AllowEdit = false;
        }

        view.OptionsBehavior.Editable = false;
        view.OptionsView.ShowGroupPanel = false;
        view.OptionsSelection.EnableAppearanceFocusedCell = false;
    }

    private static string SplitCamelCase(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        var result = System.Text.RegularExpressions.Regex.Replace(input, "([a-z])([A-Z])", "$1 $2");
        return result;
    }

    private async Task LoadAndBindOrdersAsync()
    {
        try
        {
            SetLoading(true);

            var list = await _orderRepository.GetOrdersAsync();

            gridControl.DataSource = list;
            gridView.RefreshData();
            if (cardView != null)
            {
                cardView.RefreshData();
            }
        }
        catch (Exception ex)
        {
            DevExpress.XtraEditors.XtraMessageBox.Show(this, ex.Message, "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
        }
        finally
        {
            SetLoading(false);
        }
    }

    private static string MapStatus(int s)
    {
        return s switch
        {
            0 => "Pending",
            1 => "Confirmed",
            2 => "Shipped",
            3 => "Delivered",
            4 => "Cancelled",
            _ => s.ToString()
        };
    }

    private void SetLoading(bool isLoading)
    {
        // minimal UI feedback
        Enabled = !isLoading;
    }

    private void BbiToggleView_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
    {
        if (gridControl.MainView == gridView)
        {
            if (cardView.Columns.Count == 0)
            {
                ConfigureCardView();
            }

            gridControl.MainView = cardView;
            cardView.RefreshData();
            if (barStaticViewMode != null) barStaticViewMode.Caption = "View: Cards";
        }
        else
        {
            gridControl.MainView = gridView;
            gridView.RefreshData();
            if (barStaticViewMode != null) barStaticViewMode.Caption = "View: Grid";
        }
    }

    private void ConfigureCardView()
    {
        var view = cardView;
        view.Columns.Clear();

        var properties = typeof(OrderListGridItem).GetProperties();
        int visibleIndex = 0;
        foreach (var prop in properties)
        {
            var column = view.Columns.AddVisible(prop.Name, SplitCamelCase(prop.Name));
            column.Visible = true;
            // set order for most important fields first
            column.VisibleIndex = visibleIndex++;
            if (prop.PropertyType == typeof(DateTime) || prop.PropertyType == typeof(DateTime?))
            {
                column.DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
                column.DisplayFormat.FormatString = "g";
            }
            else if (prop.PropertyType == typeof(decimal) || prop.PropertyType == typeof(double) || prop.PropertyType == typeof(float))
            {
                column.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
                column.DisplayFormat.FormatString = "c";
            }
            column.OptionsColumn.AllowEdit = false;
            // make the caption more prominent for key fields
            if (prop.Name == "CustomerName")
            {
                column.AppearanceCell.Font = new System.Drawing.Font(column.AppearanceCell.Font, System.Drawing.FontStyle.Bold);
            }
        }

        view.OptionsBehavior.Editable = false;
        // Improve card appearance (use properties available on CardView)
        view.CardWidth = 320;
        view.OptionsView.ShowCardCaption = true;
        view.OptionsView.ShowCardExpandButton = false;
        view.OptionsView.ShowHorzScrollBar = false;

        // Use repository picture for a placeholder image column if repository item exists
        try
        {
            var picCol = view.Columns.AddVisible("_Picture", "");
            picCol.UnboundType = DevExpress.Data.UnboundColumnType.Object;
            picCol.ColumnEdit = gridControl.RepositoryItems.OfType<DevExpress.XtraEditors.Repository.RepositoryItemPictureEdit>().FirstOrDefault();
            picCol.VisibleIndex = -1; // hide in order but available in template
        }
        catch
        {
            // ignore if repository item not available
        }
    }
}

