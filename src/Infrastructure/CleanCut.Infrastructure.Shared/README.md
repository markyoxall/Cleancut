# CleanCut.Infrastructure.Shared - Cross-Cutting Infrastructure Services

## Purpose in Clean Architecture

The **Infrastructure.Shared Layer** contains **cross-cutting infrastructure services** that are used across multiple layers and applications. These are technical concerns that don't belong in the core business logic but are essential for the application's operation. This layer implements service interfaces defined in the Application Layer.

## Key Principles

### 1. **Cross-Cutting Concerns**
- Services used by multiple parts of the application
- Technical infrastructure that supports business operations
- Reusable components across different contexts

### 2. **External Integration**
- Integration with third-party services (email, SMS, payment gateways)
- Cloud services integration (Azure, AWS)
- External APIs and web services

### 3. **Performance & Reliability**
- Caching implementations
- Logging and monitoring
- Retry policies and circuit breakers

### 4. **Configuration-Driven**
- Environment-specific configurations
- Feature flags and toggles
- Configurable service behaviors

## Folder Structure

```
CleanCut.Infrastructure.Shared/
??? Services/           # Shared service implementations
?   ??? EmailService.cs
?   ??? SmsService.cs
?   ??? FileStorageService.cs
?   ??? NotificationService.cs
?   ??? PaymentService.cs
??? Logging/            # Logging infrastructure
?   ??? LoggingService.cs
?   ??? ApplicationLogger.cs
?   ??? LoggingExtensions.cs
??? Email/              # Email-related services
?   ??? EmailTemplateService.cs
?   ??? SmtpEmailService.cs
?   ??? SendGridEmailService.cs
?   ??? Templates/
??? FileStorage/        # File storage services
?   ??? LocalFileStorageService.cs
?   ??? AzureBlobStorageService.cs
?   ??? AwsS3StorageService.cs
?   ??? FileStorageExtensions.cs
??? Caching/            # Caching implementations
?   ??? MemoryCacheService.cs
?   ??? RedisCacheService.cs
?   ??? CacheKeyBuilder.cs
??? External/           # External service integrations
?   ??? PaymentGateway/
?   ??? SmsProvider/
?   ??? NotificationHub/
??? Configuration/      # Service configurations
    ??? EmailSettings.cs
    ??? FileStorageSettings.cs
    ??? CacheSettings.cs
```

## What Goes Here

### Communication Services
- Email sending (SMTP, SendGrid, etc.)
- SMS notifications
- Push notifications
- Real-time messaging

### Storage Services
- File upload and management
- Image processing
- Document generation
- Cloud storage integration

### Caching Services
- In-memory caching
- Distributed caching (Redis)
- Cache invalidation strategies

### Logging & Monitoring
- Structured logging
- Application monitoring
- Performance tracking
- Error reporting

## Example Patterns

### Email Service Implementation
```csharp
public class EmailService : IEmailService
{
    private readonly IEmailProvider _emailProvider;
    private readonly IEmailTemplateService _templateService;
    private readonly EmailSettings _settings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(
        IEmailProvider emailProvider,
        IEmailTemplateService templateService,
        IOptions<EmailSettings> settings,
        ILogger<EmailService> logger)
    {
        _emailProvider = emailProvider;
        _templateService = templateService;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<EmailResult> SendAsync(EmailRequest request)
    {
        try
        {
            _logger.LogInformation("Sending email to {Recipient}", request.To);

            var emailMessage = new EmailMessage
            {
                To = request.To,
                Subject = request.Subject,
                Body = request.Body,
                IsHtml = request.IsHtml,
                From = _settings.DefaultFromAddress
            };

            var result = await _emailProvider.SendAsync(emailMessage);
            
            _logger.LogInformation("Email sent successfully to {Recipient}", request.To);
            
            return EmailResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Recipient}", request.To);
            return EmailResult.Failure(ex.Message);
        }
    }

    public async Task<EmailResult> SendTemplateAsync(string templateName, object model, string to)
    {
        try
        {
            var template = await _templateService.GetTemplateAsync(templateName);
            var renderedContent = await _templateService.RenderAsync(template, model);

            var request = new EmailRequest
            {
                To = to,
                Subject = renderedContent.Subject,
                Body = renderedContent.Body,
                IsHtml = true
            };

            return await SendAsync(request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send template email {Template} to {Recipient}", templateName, to);
            return EmailResult.Failure(ex.Message);
        }
    }
}

// Multiple email providers
public class SendGridEmailProvider : IEmailProvider
{
    private readonly SendGridClient _client;
    private readonly ILogger<SendGridEmailProvider> _logger;

    public async Task<bool> SendAsync(EmailMessage message)
    {
        var sendGridMessage = MailHelper.CreateSingleEmail(
            new EmailAddress(message.From),
            new EmailAddress(message.To),
            message.Subject,
            message.IsHtml ? null : message.Body,
            message.IsHtml ? message.Body : null);

        var response = await _client.SendEmailAsync(sendGridMessage);
        return response.IsSuccessStatusCode;
    }
}

public class SmtpEmailProvider : IEmailProvider
{
    private readonly SmtpSettings _settings;
    private readonly ILogger<SmtpEmailProvider> _logger;

    public async Task<bool> SendAsync(EmailMessage message)
    {
        using var client = new SmtpClient(_settings.Host, _settings.Port);
        client.EnableSsl = _settings.EnableSsl;
        client.Credentials = new NetworkCredential(_settings.Username, _settings.Password);

        var mailMessage = new MailMessage(message.From, message.To, message.Subject, message.Body)
        {
            IsBodyHtml = message.IsHtml
        };

        await client.SendMailAsync(mailMessage);
        return true;
    }
}
```

### File Storage Service
```csharp
public class FileStorageService : IFileStorageService
{
    private readonly IFileStorageProvider _provider;
    private readonly FileStorageSettings _settings;
    private readonly ILogger<FileStorageService> _logger;

    public async Task<FileUploadResult> UploadAsync(IFormFile file, string containerName = null)
    {
        try
        {
            // Validate file
            var validationResult = ValidateFile(file);
            if (!validationResult.IsValid)
                return FileUploadResult.Failure(validationResult.Errors);

            // Generate unique filename
            var fileName = GenerateUniqueFileName(file.FileName);
            var container = containerName ?? _settings.DefaultContainer;

            // Upload file
            using var stream = file.OpenReadStream();
            var url = await _provider.UploadAsync(stream, container, fileName, file.ContentType);

            _logger.LogInformation("File uploaded successfully: {FileName} to {Container}", fileName, container);

            return FileUploadResult.Success(new FileMetadata
            {
                FileName = fileName,
                OriginalName = file.FileName,
                Url = url,
                Size = file.Length,
                ContentType = file.ContentType,
                Container = container,
                UploadedAt = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload file: {FileName}", file.FileName);
            return FileUploadResult.Failure(ex.Message);
        }
    }

    public async Task<Stream> DownloadAsync(string container, string fileName)
    {
        return await _provider.DownloadAsync(container, fileName);
    }

    public async Task<bool> DeleteAsync(string container, string fileName)
    {
        try
        {
            await _provider.DeleteAsync(container, fileName);
            _logger.LogInformation("File deleted: {FileName} from {Container}", fileName, container);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete file: {FileName} from {Container}", fileName, container);
            return false;
        }
    }

    private ValidationResult ValidateFile(IFormFile file)
    {
        var errors = new List<string>();

        if (file.Length > _settings.MaxFileSize)
            errors.Add($"File size exceeds maximum allowed size of {_settings.MaxFileSize} bytes");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!_settings.AllowedExtensions.Contains(extension))
            errors.Add($"File type '{extension}' is not allowed");

        return new ValidationResult(errors);
    }

    private string GenerateUniqueFileName(string originalFileName)
    {
        var extension = Path.GetExtension(originalFileName);
        var fileName = Path.GetFileNameWithoutExtension(originalFileName);
        return $"{fileName}_{Guid.NewGuid()}{extension}";
    }
}

// Azure Blob Storage Provider
public class AzureBlobStorageProvider : IFileStorageProvider
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly AzureStorageSettings _settings;

    public async Task<string> UploadAsync(Stream stream, string containerName, string fileName, string contentType)
    {
        var containerClient = await GetContainerClientAsync(containerName);
        var blobClient = containerClient.GetBlobClient(fileName);

        await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = contentType });
        
        return blobClient.Uri.ToString();
    }

    public async Task<Stream> DownloadAsync(string containerName, string fileName)
    {
        var containerClient = await GetContainerClientAsync(containerName);
        var blobClient = containerClient.GetBlobClient(fileName);

        var response = await blobClient.DownloadAsync();
        return response.Value.Content;
    }

    public async Task DeleteAsync(string containerName, string fileName)
    {
        var containerClient = await GetContainerClientAsync(containerName);
        var blobClient = containerClient.GetBlobClient(fileName);

        await blobClient.DeleteIfExistsAsync();
    }

    private async Task<BlobContainerClient> GetContainerClientAsync(string containerName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        await containerClient.CreateIfNotExistsAsync();
        return containerClient;
    }
}
```

### Caching Service
```csharp
public class CacheService : ICacheService
{
    private readonly IDistributedCache _distributedCache;
    private readonly IMemoryCache _memoryCache;
    private readonly CacheSettings _settings;
    private readonly ILogger<CacheService> _logger;

    public async Task<T> GetAsync<T>(string key) where T : class
    {
        try
        {
            // Try memory cache first
            if (_memoryCache.TryGetValue(key, out T memoryCachedValue))
            {
                _logger.LogDebug("Cache hit (memory): {Key}", key);
                return memoryCachedValue;
            }

            // Try distributed cache
            var cachedValue = await _distributedCache.GetStringAsync(key);
            if (cachedValue != null)
            {
                var deserializedValue = JsonSerializer.Deserialize<T>(cachedValue);
                
                // Store in memory cache for faster access
                _memoryCache.Set(key, deserializedValue, TimeSpan.FromMinutes(5));
                
                _logger.LogDebug("Cache hit (distributed): {Key}", key);
                return deserializedValue;
            }

            _logger.LogDebug("Cache miss: {Key}", key);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cached value for key: {Key}", key);
            return null;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
    {
        try
        {
            var expirationTime = expiration ?? TimeSpan.FromMinutes(_settings.DefaultExpirationMinutes);
            
            // Set in memory cache
            _memoryCache.Set(key, value, expirationTime);
            
            // Set in distributed cache
            var serializedValue = JsonSerializer.Serialize(value);
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expirationTime
            };
            
            await _distributedCache.SetStringAsync(key, serializedValue, options);
            
            _logger.LogDebug("Cache set: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting cached value for key: {Key}", key);
        }
    }

    public async Task RemoveAsync(string key)
    {
        try
        {
            _memoryCache.Remove(key);
            await _distributedCache.RemoveAsync(key);
            
            _logger.LogDebug("Cache removed: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cached value for key: {Key}", key);
        }
    }

    public async Task RemoveByPatternAsync(string pattern)
    {
        // Implementation depends on cache provider
        // Redis supports pattern-based removal, memory cache requires tracking keys
    }
}
```

### Logging Service
```csharp
public class ApplicationLogger : IApplicationLogger
{
    private readonly ILogger<ApplicationLogger> _logger;
    private readonly TelemetryClient _telemetryClient;

    public void LogInformation(string message, params object[] args)
    {
        _logger.LogInformation(message, args);
    }

    public void LogWarning(string message, params object[] args)
    {
        _logger.LogWarning(message, args);
    }

    public void LogError(Exception exception, string message, params object[] args)
    {
        _logger.LogError(exception, message, args);
        _telemetryClient?.TrackException(exception);
    }

    public void LogBusinessEvent(string eventName, object data)
    {
        var properties = ConvertToDictionary(data);
        _logger.LogInformation("Business Event: {EventName} {@Data}", eventName, data);
        _telemetryClient?.TrackEvent(eventName, properties);
    }

    public void LogPerformance(string operationName, TimeSpan duration, bool success = true)
    {
        _logger.LogInformation("Performance: {Operation} took {Duration}ms, Success: {Success}", 
            operationName, duration.TotalMilliseconds, success);
            
        _telemetryClient?.TrackDependency("Operation", operationName, 
            DateTime.UtcNow.Subtract(duration), duration, success);
    }
}
```

## Key Technologies & Packages

### Required NuGet Packages
```xml
<!-- Email -->
<PackageReference Include="SendGrid" Version="9.28.1" />
<PackageReference Include="MailKit" Version="4.2.0" />

<!-- File Storage -->
<PackageReference Include="Azure.Storage.Blobs" Version="12.18.0" />
<PackageReference Include="AWSSDK.S3" Version="3.7.307" />

<!-- Caching -->
<PackageReference Include="Microsoft.Extensions.Caching.StackExchangeRedis" Version="8.0.0" />
<PackageReference Include="Microsoft.Extensions.Caching.Memory" Version="8.0.0" />

<!-- Logging -->
<PackageReference Include="Serilog.AspNetCore" Version="7.0.0" />
<PackageReference Include="Microsoft.ApplicationInsights.AspNetCore" Version="2.21.0" />

<!-- HTTP Clients -->
<PackageReference Include="Microsoft.Extensions.Http.Polly" Version="8.0.0" />
<PackageReference Include="Polly" Version="7.2.4" />
```

### Project References
- **CleanCut.Application** (implements service interfaces)

## Configuration Examples

### Email Settings
```json
{
  "EmailSettings": {
    "DefaultFromAddress": "noreply@cleancut.com",
    "DefaultFromName": "CleanCut Application",
    "Provider": "SendGrid", // or "Smtp"
    "SendGrid": {
      "ApiKey": "your-sendgrid-api-key"
    },
    "Smtp": {
      "Host": "smtp.gmail.com",
      "Port": 587,
      "Username": "your-email@gmail.com",
      "Password": "your-password",
      "EnableSsl": true
    }
  }
}
```

### File Storage Settings
```json
{
  "FileStorageSettings": {
    "Provider": "AzureBlob", // or "Local", "AwsS3"
    "DefaultContainer": "uploads",
    "MaxFileSize": 10485760, // 10MB
    "AllowedExtensions": [".jpg", ".jpeg", ".png", ".pdf", ".docx"],
    "AzureBlob": {
      "ConnectionString": "your-azure-storage-connection-string"
    },
    "Local": {
      "BasePath": "wwwroot/uploads"
    }
  }
}
```

## Integration Patterns

### Dependency Injection Setup
```csharp
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSharedInfrastructure(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // Email services
        services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IEmailTemplateService, EmailTemplateService>();
        
        var emailProvider = configuration.GetValue<string>("EmailSettings:Provider");
        switch (emailProvider?.ToLower())
        {
            case "sendgrid":
                services.AddScoped<IEmailProvider, SendGridEmailProvider>();
                break;
            case "smtp":
                services.AddScoped<IEmailProvider, SmtpEmailProvider>();
                break;
        }

        // File storage
        services.Configure<FileStorageSettings>(configuration.GetSection("FileStorageSettings"));
        services.AddScoped<IFileStorageService, FileStorageService>();
        
        var storageProvider = configuration.GetValue<string>("FileStorageSettings:Provider");
        switch (storageProvider?.ToLower())
        {
            case "azureblob":
                services.AddScoped<IFileStorageProvider, AzureBlobStorageProvider>();
                break;
            case "local":
                services.AddScoped<IFileStorageProvider, LocalFileStorageProvider>();
                break;
        }

        // Caching
        services.Configure<CacheSettings>(configuration.GetSection("CacheSettings"));
        services.AddMemoryCache();
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis");
        });
        services.AddScoped<ICacheService, CacheService>();

        // Logging
        services.AddScoped<IApplicationLogger, ApplicationLogger>();

        return services;
    }
}
```

## Common Patterns

### Circuit Breaker Pattern
```csharp
public class HttpClientService : IHttpClientService
{
    private readonly HttpClient _httpClient;
    private readonly IAsyncPolicy<HttpResponseMessage> _retryPolicy;

    public HttpClientService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _retryPolicy = Policy
            .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    // Log retry attempt
                });
    }

    public async Task<T> GetAsync<T>(string endpoint)
    {
        var response = await _retryPolicy.ExecuteAsync(async () =>
        {
            return await _httpClient.GetAsync(endpoint);
        });

        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(content);
    }
}
```

## Common Mistakes to Avoid

? **Tight Coupling** - Don't reference specific implementations from other layers
? **Missing Configuration** - Always make services configurable
? **No Error Handling** - Implement proper error handling and logging
? **Performance Issues** - Consider caching and async patterns
? **Security Vulnerabilities** - Validate inputs and secure communications

? **Provider Pattern** - Support multiple implementations (email, storage, etc.)
? **Configuration-Driven** - Make services configurable for different environments
? **Resilience Patterns** - Implement retry, circuit breaker, and timeout patterns
? **Proper Logging** - Log important events and errors for monitoring
? **Interface Abstractions** - Keep implementations behind interfaces

This layer provides the technical foundation that supports your business operations - keep it reliable, configurable, and well-tested!