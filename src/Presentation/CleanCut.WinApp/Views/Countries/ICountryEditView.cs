using CleanCut.WinApp.Views.Countries;

namespace CleanCut.WinApp.Views.Countries;

public interface ICountryEditView : MVP.IView
{

    /// <summary>
    /// Event raised when user wants to save
    /// </summary>
    event EventHandler? SaveRequested;

    /// <summary>
    /// Event raised when user wants to cancel
    /// </summary>
    event EventHandler? CancelRequested;

    /// <summary>
    /// Get the user data from the form
    /// </summary>
    CountryEditViewModel GetCountryData();

    /// <summary>
    /// Set the user data in the form
    /// </summary>
    void SetCountryData(CountryEditViewModel user);

    /// <summary>
    /// Validate the form and return validation errors
    /// </summary>
    Dictionary<string, string> ValidateForm();

    /// <summary>
    /// Clear all form fields
    /// </summary>
    void ClearForm();

}
