namespace CleanCut.Infrastructure.BackgroundServices.ProductExport;

/// <summary>
/// Configuration settings for the product export background service
/// </summary>
public class ProductExportConfiguration
{
    /// <summary>
    /// How often to run the product export (in minutes)
    /// </summary>
    public int IntervalMinutes { get; set; } = 5;

    /// <summary>
    /// Number of retry attempts when API calls fail
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// Delay between retry attempts (in seconds)
    /// </summary>
    public int RetryDelaySeconds { get; set; } = 30;

    /// <summary>
    /// Maximum delay between retry attempts (in seconds) for exponential backoff
    /// </summary>
    public int MaxRetryDelaySeconds { get; set; } = 300;

    /// <summary>
    /// Whether to continue running after errors or stop the service
    /// </summary>
    public bool ContinueOnError { get; set; } = true;

    /// <summary>
    /// Initial delay before starting the export process (in seconds)
    /// </summary>
    public int InitialDelaySeconds { get; set; } = 30;

    /// <summary>
    /// Whether to skip export if API is unavailable
    /// </summary>
    public bool SkipOnApiUnavailable { get; set; } = true;

    /// <summary>
    /// Timeout for health check requests (in seconds)
    /// </summary>
    public int HealthCheckTimeoutSeconds { get; set; } = 10;
}