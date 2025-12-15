namespace CleanCut.BackgroundService.Configuration;

/// <summary>
/// Configuration settings for the CleanCut Background Service
/// </summary>
public class BackgroundServiceSettings
{
    public const string SectionName = "BackgroundService";

    /// <summary>
    /// IdentityServer configuration for client credentials authentication
    /// </summary>
    public IdentityServerSettings IdentityServer { get; set; } = new();

    /// <summary>
    /// API configuration for calling the CleanCut API
    /// </summary>
    public ApiSettings Api { get; set; } = new();

    /// <summary>
 /// CSV export configuration
    /// </summary>
    public CsvSettings Csv { get; set; } = new();

    /// <summary>
    /// How often to run the product export (in minutes)
    /// </summary>
  public int IntervalMinutes { get; set; } = 60; // Default: run every hour
}

/// <summary>
/// IdentityServer authentication settings
/// </summary>
public class IdentityServerSettings
{
  /// <summary>
    /// IdentityServer authority URL
    /// </summary>
    public string Authority { get; set; } = "https://localhost:5001";

    /// <summary>
    /// Client ID for client credentials flow
    /// </summary>
    public string ClientId { get; set; } = "cleancut-background-service";

    /// <summary>
    /// Client secret for authentication
  /// </summary>
    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>
  /// Scopes to request
    /// </summary>
    public string Scope { get; set; } = "CleanCutAPI";

    /// <summary>
    /// Token endpoint URL (will be discovered if not specified)
    /// </summary>
    public string? TokenEndpoint { get; set; }
}

/// <summary>
/// API configuration settings
/// </summary>
public class ApiSettings
{
    /// <summary>
    /// Base URL of the CleanCut API
    /// </summary>
public string BaseUrl { get; set; } = "https://localhost:7142";

    /// <summary>
    /// Products endpoint path
    /// </summary>
    public string ProductsEndpoint { get; set; } = "/api/v1/products";

    /// <summary>
 /// Timeout for API calls (in seconds)
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;
}

/// <summary>
/// CSV export settings
/// </summary>
public class CsvSettings
{
    /// <summary>
    /// Directory to save CSV files (relative to application directory)
    /// </summary>
    public string OutputDirectory { get; set; } = "exports";

    /// <summary>
    /// CSV filename pattern (supports DateTime formatting)
 /// </summary>
public string FileNamePattern { get; set; } = "products_{0:yyyyMMdd_HHmmss}.csv";

    /// <summary>
    /// Whether to include headers in CSV
  /// </summary>
    public bool IncludeHeaders { get; set; } = true;

    /// <summary>
    /// Whether to keep old CSV files or overwrite
    /// </summary>
 public bool KeepOldFiles { get; set; } = true;

    /// <summary>
    /// Maximum number of old files to keep (if KeepOldFiles is true)
    /// </summary>
    public int MaxOldFiles { get; set; } = 10;
}