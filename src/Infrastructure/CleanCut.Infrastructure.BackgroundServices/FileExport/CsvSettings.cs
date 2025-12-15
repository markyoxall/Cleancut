namespace CleanCut.Infrastructure.BackgroundServices.FileExport;

/// <summary>
/// CSV export settings
/// </summary>
public class CsvSettings
{
    /// <summary>
    /// Output directory for CSV files
/// </summary>
  public string OutputDirectory { get; set; } = "exports";

    /// <summary>
    /// File name pattern with placeholder for timestamp
    /// </summary>
    public string FileNamePattern { get; set; } = "products_{0:yyyyMMdd_HHmmss}.csv";

    /// <summary>
    /// Whether to include headers in CSV
  /// </summary>
    public bool IncludeHeaders { get; set; } = true;

    /// <summary>
 /// Whether to keep old export files
    /// </summary>
    public bool KeepOldFiles { get; set; } = true;

    /// <summary>
    /// Whether CSV export is enabled. Set to false to disable writing CSV files (useful for development).
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Maximum number of old files to keep
    /// </summary>
    public int MaxOldFiles { get; set; } = 10;

    /// <summary>
    /// CSV field delimiter
    /// </summary>
  public string Delimiter { get; set; } = ",";

/// <summary>
    /// Text qualifier for fields containing delimiters
    /// </summary>
    public string TextQualifier { get; set; } = "\"";
}
