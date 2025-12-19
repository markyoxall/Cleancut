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
            var userId = GetSelectedCountryId();
            if (userId.HasValue)
                DeleteCountryRequested?.Invoke(this, userId.Value);
        };
        _refreshButton.Click += (s, e) => RefreshRequested?.Invoke(this, EventArgs.Empty);

        _listView.SelectedIndexChanged += (s, e) => UpdateButtonStates();
    }

    public void ClearCountries()
    {
        throw new NotImplementedException();
    }

    public void DisplayCountries(IEnumerable<CountryInfo> countries)
    {
        if (InvokeRequired)
        {
            Invoke(() => DisplayCountries(countries));
            return;
        }

        _listView.Items.Clear();

        foreach (var country in countries)
        {
            var item = new ListViewItem(country.Name)
            {
                Tag = country.Id
            };
            item.SubItems.Add(country.Name);
          

            _listView.Items.Add(item);
        }

        UpdateButtonStates();
    }

    public Guid? GetSelectedCountryId()
    {
        throw new NotImplementedException();
    }

    private void UpdateButtonStates()
    {
        var hasSelection = _listView.SelectedItems.Count > 0;
        _editButton.Enabled = hasSelection;
        _deleteButton.Enabled = hasSelection;
    }

}
