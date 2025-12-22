namespace CleanCut.WinApp.Services.Management;

/// <summary>
/// Provides the current application user's username or ID.
/// In a real app, this would be set after login.
/// </summary>
public static class AppUserContext
{
    // Set this property after user login.
    public static string CurrentUserName { get; set; } = "markymark";
}
