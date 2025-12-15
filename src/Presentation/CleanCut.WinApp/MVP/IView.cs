namespace CleanCut.WinApp.MVP;

/// <summary>
/// Base interface for all views in MVP pattern
/// </summary>
public interface IView
{
    /// <summary>
    /// Shows the view
    /// </summary>
    void Show();
    
    /// <summary>
    /// Hides the view
    /// </summary>
    void Hide();
    
    /// <summary>
    /// Closes the view
    /// </summary>
    void Close();
    
    /// <summary>
    /// Shows an error message to the user
    /// </summary>
    void ShowError(string message);
    
    /// <summary>
    /// Shows an information message to the user
    /// </summary>
    void ShowInfo(string message);
    
    /// <summary>
    /// Shows a success message to the user
    /// </summary>
    void ShowSuccess(string message);
    
    /// <summary>
    /// Shows a confirmation dialog and returns the result
    /// </summary>
    bool ShowConfirmation(string message);
    
    /// <summary>
    /// Shows or hides the loading indicator
    /// </summary>
    void SetLoading(bool isLoading);
}