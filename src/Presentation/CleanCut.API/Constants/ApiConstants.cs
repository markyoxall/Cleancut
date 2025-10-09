namespace CleanCut.API.Constants;

/// <summary>
/// API configuration constants
/// </summary>
public static class ApiConstants
{
    public const string Version = "v1";
    public const string Title = "CleanCut API";
    public const string Description = "A Clean Architecture API for CleanCut application";
    
    public static string SwaggerEndpoint => $"/openapi/{Version}.json";
    public static string SwaggerTitle => $"{Title} {Version}";
}