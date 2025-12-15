using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using CleanCut.Application.DTOs;

namespace CleanCut.Infrastructure.BackgroundServices.FileExport;

/// <summary>
/// Service for exporting data to CSV files
/// </summary>
public interface ICsvExportService
{
    /// <summary>
    /// Exports products to a CSV file
    /// </summary>
    Task<string> ExportProductsAsync(IReadOnlyList<ProductInfo> products, CancellationToken cancellationToken = default);

    /// <summary>
    /// Exports orders to a CSV file
    /// </summary>
    Task<string> ExportOrdersAsync(IReadOnlyList<CleanCut.Application.DTOs.OrderInfo> orders, CancellationToken cancellationToken = default);
}

/// <summary>
/// Implementation of CSV export service using CsvHelper
/// </summary>
public class CsvExportService : ICsvExportService
{
    private readonly CsvSettings _settings;
    private readonly ILogger<CsvExportService> _logger;

    public CsvExportService(
        IOptions<CsvSettings> settings,
        ILogger<CsvExportService> logger)
    {
  _settings = settings.Value;
        _logger = logger;
    }

    public async Task<string> ExportProductsAsync(IReadOnlyList<ProductInfo> products, CancellationToken cancellationToken = default)
    {
   _logger.LogInformation("Starting CSV export of {Count} products", products.Count);

        // Respect Enabled flag
        if (!_settings.Enabled)
        {
            _logger.LogInformation("CSV export is disabled via configuration. Skipping export.");
            return string.Empty;
        }

        try
 {
      // Ensure output directory exists
          var outputDirectory = Path.GetFullPath(_settings.OutputDirectory);
            if (!Directory.Exists(outputDirectory))
     {
     Directory.CreateDirectory(outputDirectory);
         _logger.LogInformation("Created output directory: {Directory}", outputDirectory);
          }

  // Generate filename
 var filename = string.Format(_settings.FileNamePattern, DateTime.UtcNow);
            var filePath = Path.Combine(outputDirectory, filename);

      // Configure CSV settings
      var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
 HasHeaderRecord = _settings.IncludeHeaders,
                Delimiter = ",",
   Encoding = System.Text.Encoding.UTF8
            };

      // Write CSV file
  await using var writer = new StringWriter();
     await using var csv = new CsvWriter(writer, csvConfig);

            // Define the CSV mapping
 csv.Context.RegisterClassMap<ProductCsvMap>();

        // Write records
  await csv.WriteRecordsAsync(products, cancellationToken);
          var csvContent = writer.ToString();

            // Write to file
   await File.WriteAllTextAsync(filePath, csvContent, cancellationToken);

      _logger.LogInformation("Successfully exported {Count} products to CSV file: {FilePath}", products.Count, filePath);

       // Clean up old files if configured
            if (_settings.KeepOldFiles && _settings.MaxOldFiles > 0)
 {
         await CleanupOldFilesAsync(outputDirectory);
       }

            return filePath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to export products to CSV");
       throw new InvalidOperationException("Failed to export products to CSV", ex);
      }
    }

    public async Task<string> ExportOrdersAsync(IReadOnlyList<CleanCut.Application.DTOs.OrderInfo> orders, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting CSV export of {Count} orders", orders.Count);

        if (!_settings.Enabled)
        {
            _logger.LogInformation("CSV export is disabled via configuration. Skipping export.");
            return string.Empty;
        }

        try
        {
            var outputDirectory = Path.GetFullPath(_settings.OutputDirectory);
            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
                _logger.LogInformation("Created output directory: {Directory}", outputDirectory);
            }

            // Generate filename for orders
            var filename = string.Format(_settings.FileNamePattern.Replace("products", "orders"), DateTime.UtcNow);
            var filePath = Path.Combine(outputDirectory, filename);

            // Configure CSV settings
            var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = _settings.IncludeHeaders,
                Delimiter = _settings.Delimiter,
                Encoding = System.Text.Encoding.UTF8
            };

            // Write CSV file
            await using var writer = new StringWriter();
            await using var csv = new CsvWriter(writer, csvConfig);

            // Define the CSV mapping
            csv.Context.RegisterClassMap<OrderCsvMap>();

            // Write records
            await csv.WriteRecordsAsync(orders, cancellationToken);
            var csvContent = writer.ToString();

            // Write to file
            await File.WriteAllTextAsync(filePath, csvContent, cancellationToken);

            _logger.LogInformation("Successfully exported {Count} orders to CSV file: {FilePath}", orders.Count, filePath);

            // Clean up old files if configured
            if (_settings.KeepOldFiles && _settings.MaxOldFiles > 0)
            {
                await CleanupOldFilesAsync(outputDirectory);
            }

            return filePath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to export orders to CSV");
            throw new InvalidOperationException("Failed to export orders to CSV", ex);
        }
    }

    private async Task CleanupOldFilesAsync(string directory)
    {
        try
   {
        var files = Directory.GetFiles(directory, "products_*.csv")
     .Select(f => new FileInfo(f))
             .OrderByDescending(f => f.CreationTime)
        .ToList();

            if (files.Count > _settings.MaxOldFiles)
            {
           var filesToDelete = files.Skip(_settings.MaxOldFiles);
     foreach (var file in filesToDelete)
     {
         File.Delete(file.FullName);
      _logger.LogInformation("Deleted old CSV file: {FileName}", file.Name);
        }
    }
      }
        catch (Exception ex)
        {
  _logger.LogWarning(ex, "Failed to cleanup old CSV files");
        // Don't throw - this is not critical
      }
    }
}

/// <summary>
/// CSV mapping configuration for ProductInfo
/// </summary>
public sealed class ProductCsvMap : ClassMap<ProductInfo>
{
    public ProductCsvMap()
    {
        Map(m => m.Id).Name("Product ID");
        Map(m => m.Name).Name("Product Name");
  Map(m => m.Description).Name("Description");
      Map(m => m.CustomerId).Name("Customer ID");
        // Don't include these raw fields in CSV
    Map(m => m.Price).Ignore();
   Map(m => m.IsAvailable).Ignore();
        Map(m => m.CreatedAt).Ignore();
    Map(m => m.UpdatedAt).Ignore();
    }
}

/// <summary>
/// CSV mapping configuration for OrderInfo
/// </summary>
public sealed class OrderCsvMap : ClassMap<CleanCut.Application.DTOs.OrderInfo>
{
    public OrderCsvMap()
    {
        Map(m => m.Id).Name("Order ID");
        Map(m => m.OrderNumber).Name("Order Number");
        Map(m => m.OrderDate).Name("Order Date");
        Map(m => m.CustomerName).Name("Customer Name");
        Map(m => m.CustomerEmail).Name("Customer Email");
        Map(m => m.TotalAmount).Name("Total Amount");
        Map(m => m.TotalItemCount).Name("Total Items");
        Map(m => m.StatusName).Name("Status");
        Map(m => m.ShippingAddress).Name("Shipping Address");
        Map(m => m.BillingAddress).Name("Billing Address");
        Map(m => m.Notes).Name("Notes");
    }
}
