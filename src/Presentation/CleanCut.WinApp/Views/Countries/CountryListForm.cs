using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CleanCut.Application.DTOs;
using CleanCut.WinApp.MVP;

namespace CleanCut.WinApp.Views.Countries;

/// <summary>
/// Country list form implementing <see cref="ICountryListView"/>. The view is kept thin:
/// it exposes events for layout save/load and provides methods to get/apply layout JSON.
/// Persistence is handled by the presenter via <see cref="ILayoutPersistenceService"/>.
/// </summary>
public partial class CountryListForm : BaseForm, ICountryListView
{
    /// <summary>Raised when the user requests to add a country.</summary>
    public event EventHandler? AddCountryRequested;

    /// <summary>Raised when the user requests to edit a country.</summary>
    public event EventHandler<Guid>? EditCountryRequested;

    /// <summary>Raised when the user requests to delete a country.</summary>
    public event EventHandler<Guid>? DeleteCountryRequested;

    /// <summary>Raised when the user requests to refresh the list.</summary>
    public event EventHandler? RefreshRequested;

    /// <summary>Raised when the user requests saving the current grid layout. Presenter should persist.</summary>
    public event EventHandler? SaveLayoutRequested;

    /// <summary>Raised when the user requests loading/applying the saved layout. Presenter should load and instruct view to apply.</summary>
    public event EventHandler? LoadLayoutRequested;

    /// <summary>
    /// Creates a new instance of the <see cref="CountryListForm"/>.
    /// </summary>
    public CountryListForm()
    {
        InitializeComponent();
        SetupEventHandlers();
    }

    private void SetupEventHandlers()
    {
        _addButton.Click += (s, e) => AddCountryRequested?.Invoke(this, EventArgs.Empty);
        _editButton.Click += (s, e) => {
            var countryId = GetSelectedCountryId();
            if (countryId.HasValue)
                EditCountryRequested?.Invoke(this, countryId.Value);
        };
        _deleteButton.Click += (s, e) => {
            var countryId = GetSelectedCountryId();
            if (countryId.HasValue)
                DeleteCountryRequested?.Invoke(this, countryId.Value);
        };
        _refreshButton.Click += (s, e) => RefreshRequested?.Invoke(this, EventArgs.Empty);

        if (_saveLayoutButton != null)
            _saveLayoutButton.Click += (s, e) => SaveLayoutRequested?.Invoke(this, EventArgs.Empty);
        if (_loadLayoutButton != null)
            _loadLayoutButton.Click += (s, e) => LoadLayoutRequested?.Invoke(this, EventArgs.Empty);

        try
        {
            var view = _gridControl?.MainView as DevExpress.XtraGrid.Views.Base.ColumnView;
            if (view != null)
                view.FocusedRowChanged += (s, e) => UpdateButtonStates();
        }
        catch { }
    }

    /// <inheritdoc />
    public void ClearCountries()
    {
        if (InvokeRequired)
        {
            Invoke((Action)ClearCountries);
            return;
        }
        _gridControl.DataSource = null;
    }

    /// <inheritdoc />
    public void DisplayCountries(System.Collections.Generic.IEnumerable<CountryInfo> countries)
    {
        if (InvokeRequired)
        {
            Invoke(() => DisplayCountries(countries));
            return;
        }

        _gridControl.DataSource = countries.ToList();
        UpdateButtonStates();
    }

    /// <inheritdoc />
    public Guid? GetSelectedCountryId()
    {
        try
        {
            var view = _gridControl?.MainView as DevExpress.XtraGrid.Views.Base.ColumnView;
            if (view == null || view.FocusedRowHandle < 0) return null;
            var row = view.GetRow(view.FocusedRowHandle) as CountryInfo;
            return row?.Id;
        }
        catch
        {
            return null;
        }
    }

    private void UpdateButtonStates()
    {
        try
        {
            var view = _gridControl?.MainView as DevExpress.XtraGrid.Views.Base.ColumnView;
            var hasSelection = view != null && view.FocusedRowHandle >= 0 && view.GetRow(view.FocusedRowHandle) is CountryInfo;
            _editButton.Enabled = hasSelection;
            _deleteButton.Enabled = hasSelection;
        }
        catch
        {
            _editButton.Enabled = false;
            _deleteButton.Enabled = false;
        }
    }

    /// <inheritdoc />
    public string? GetLayoutJson()
    {
        try
        {
            var view = _gridControl?.MainView;
            if (view == null) return null;
            using var ms = new MemoryStream();
            view.SaveLayoutToStream(ms);
            ms.Position = 0;
            using var sr = new StreamReader(ms, Encoding.UTF8);
            return sr.ReadToEnd();
        }
        catch
        {
            return null;
        }
    }

    /// <inheritdoc />
    public void ApplyLayoutFromJson(string layoutJson)
    {
        if (string.IsNullOrEmpty(layoutJson)) return;

        try
        {
            if (InvokeRequired)
            {
                Invoke(() => ApplyLayoutFromJson(layoutJson));
                return;
            }

            var view = _gridControl?.MainView;
            if (view == null) return;

            var bytes = Encoding.UTF8.GetBytes(layoutJson);
            using var ms = new MemoryStream(bytes);
            ms.Position = 0;
            view.RestoreLayoutFromStream(ms);
        }
        catch
        {
            // ignore; presenter surfaces errors to user
        }
    }
}
