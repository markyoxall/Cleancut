using CleanCut.Application.DTOs;

namespace CleanCut.WinApp.Views.Countries;

public interface ICountryListView : MVP.IView
{

    /// </summary>
    event EventHandler? AddCountryRequested;

    /// <summary>
    /// Event raised when user wants to edit a user
    /// </summary>
    event EventHandler<Guid>? EditCountryRequested;

    /// <summary>
    /// Event raised when user wants to delete a user
    /// </summary>
    event EventHandler<Guid>? DeleteCountryRequested;

    /// <summary>
    /// Event raised when user wants to refresh the list
    /// </summary>
    event EventHandler? RefreshRequested;

    /// <summary>
    /// Display the list of countries
    /// </summary>
    void DisplayCountries(IEnumerable<CountryInfo> countries);

    /// <summary>
    /// Clear the countries list
    /// </summary>
    void ClearCountries();

    /// <summary>
    /// Get the selected user ID
    /// </summary>
    Guid? GetSelectedCountryId();
}
