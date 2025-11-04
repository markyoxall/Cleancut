namespace CleanCut.Infrastructure.BackgroundServices.ExternalApi;

/// <summary>
/// API settings for external API calls
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
    /// Request timeout in seconds
    /// </summary>
  public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Maximum number of retry attempts
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// Health check endpoint for API availability
    /// </summary>
    public string? HealthCheckEndpoint { get; set; }
}