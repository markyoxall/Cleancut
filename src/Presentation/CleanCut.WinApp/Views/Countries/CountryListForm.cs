using CleanCut.Application.DTOs;
using CleanCut.WinApp.MVP;
using CleanCut.WinApp.Views.Countries;

namespace CleanCut.WinApp.Views.Countries;

public partial class CountryListForm : BaseForm, ICountryListView
{
    public event EventHandler? AddCountryRequested;
    public event EventHandler<Guid>? EditCountryRequested;
    public event EventHandler<Guid>? DeleteCountryRequested;
    public event EventHandler? RefreshRequested;


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

        // Use GridView selection event
        _gridView.FocusedRowChanged += (s, e) => UpdateButtonStates();
    }

    public void ClearCountries()
    {
        if (InvokeRequired)
        {
            Invoke((Action)ClearCountries);
            return;
        }
        _gridControl.DataSource = null;
    }

    public void DisplayCountries(IEnumerable<CountryInfo> countries)
    {
        if (InvokeRequired)
        {
            Invoke(() => DisplayCountries(countries));
            return;
        }
        // Bind the list to the grid
        _gridControl.DataSource = countries.ToList();
        UpdateButtonStates();
    }

    public Guid? GetSelectedCountryId()
    {
        if (_gridView.FocusedRowHandle < 0)
            return null;
        var row = _gridView.GetRow(_gridView.FocusedRowHandle) as CountryInfo;
        return row?.Id;
    }

    private void UpdateButtonStates()
    {
        var hasSelection = _gridView.FocusedRowHandle >= 0 && _gridView.GetRow(_gridView.FocusedRowHandle) is CountryInfo;
        _editButton.Enabled = hasSelection;
        _deleteButton.Enabled = hasSelection;
    }

}
