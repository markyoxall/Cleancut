namespace CleanCut.API.Constants;

/// <summary>
/// API configuration constants
/// </summary>
public static class ApiConstants
{
    // Primary API/version info
    public const string Version = "v1";
    public const string Title = "CleanCut API";
    public const string Description = "A Clean Architecture API for CleanCut application";

    // OpenAPI / Swagger endpoints used by the application at runtime.
    // Program.cs currently maps OpenAPI at "/openapi/v1.json" and exposes Swagger UI at "/swagger".
    public static string OpenApiJson => $"/openapi/{Version}.json";
    public static string SwaggerJson => $"/swagger/{Version}/swagger.json";
    public const string SwaggerUiRoutePrefix = "swagger";

    // Backwards-compatible helpers
    public static string SwaggerEndpoint => OpenApiJson;
    public static string SwaggerTitle => $"{Title} {Version}";
    public static string SwaggerUiTitle => $"{Title} - API Documentation (V1 & V2)";
}