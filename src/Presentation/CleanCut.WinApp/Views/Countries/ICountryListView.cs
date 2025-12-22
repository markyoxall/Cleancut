using CleanCut.Application.DTOs;

namespace CleanCut.WinApp.Views.Countries;

/// <summary>
/// View contract for the Country list management screen
/// </summary>
public interface ICountryListView : MVP.IView
{
    /// <summary>
    /// Event raised when user requests to add a country
    /// </summary>
    event EventHandler? AddCountryRequested;

    /// <summary>
    /// Event raised when user requests to edit a country
    /// </summary>
    event EventHandler<Guid>? EditCountryRequested;

    /// <summary>
    /// Event raised when user requests to delete a country
    /// </summary>
    event EventHandler<Guid>? DeleteCountryRequested;

    /// <summary>
    /// Event raised when user requests a refresh of the list
    /// </summary>
    event EventHandler? RefreshRequested;

    /// <summary>
    /// Event raised when user requests to save the current grid layout
    /// </summary>
    event EventHandler? SaveLayoutRequested;

    /// <summary>
    /// Event raised when user requests to load/apply a previously saved layout
    /// </summary>
    event EventHandler? LoadLayoutRequested;

    /// <summary>
    /// Display the list of countries
    /// </summary>
    void DisplayCountries(IEnumerable<CountryInfo> countries);

    /// <summary>
    /// Clear the countries list
    /// </summary>
    void ClearCountries();

    /// <summary>
    /// Get the selected country ID
    /// </summary>
    Guid? GetSelectedCountryId();

    /// <summary>
    /// Retrieve the GridView layout as a UTF-8 JSON/text string (as produced by DevExpress SaveLayoutToStream).
    /// Returns null if layout cannot be produced.
    /// </summary>
    string? GetLayoutJson();

    /// <summary>
    /// Apply the provided layout JSON/text to the GridView (uses DevExpress RestoreLayoutFromStream).
    /// </summary>
    void ApplyLayoutFromJson(string layoutJson);
}
