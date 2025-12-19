using CleanCut.WinApp.MVP;
using CleanCut.WinApp.Views.Customers;

namespace CleanCut.WinApp.Views.Countries;

public partial class CountryEditForm : BaseForm, ICountryEditView
{
    public event EventHandler? SaveRequested;
    public event EventHandler? CancelRequested;

    public void ClearForm()
    {
        throw new NotImplementedException();
    }

    public CountryEditViewModel GetCountryData()
    {
        throw new NotImplementedException();
    }

    public void SetCountryData(CountryEditViewModel user)
    {
        throw new NotImplementedException();
    }

    public Dictionary<string, string> ValidateForm()
    {
        throw new NotImplementedException();
    }
}
