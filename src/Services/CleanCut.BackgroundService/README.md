# CleanCut.BackgroundService

A .NET 9 background service that authenticates with IdentityServer using client credentials and periodically exports product data from the CleanCut API to CSV files.

## ?? Purpose

This background service demonstrates:
- **Client Credentials OAuth2 flow** for machine-to-machine authentication
- **Authenticated API calls** to protected endpoints
- **Automated CSV export** with configurable scheduling
- **Enterprise logging** and error handling

## ??? Architecture

```
??????????????????? ???????????????????     ???????????????????
?  Background     ???????  IdentityServer ???????  CleanCut API   ?
?  Service     ?     ?  (Port 5001)    ?     ?  (Port 7142)    ?
?         ?     ?     ?     ?   ?
? Client Creds    ?     ? Issues JWT      ?     ? Validates Token ?
? Authentication  ?     ? Access Token    ?     ? Returns Products?
???????????????????     ???????????????????     ???????????????????
         ?
         ?
???????????????????
?   CSV Files     ?
?   /exports/     ?
?        ?
? products_*.csv  ?
???????????????????
```

## ?? Configuration

### appsettings.json
```json
{
  "BackgroundService": {
    "IntervalMinutes": 5,
    "IdentityServer": {
      "Authority": "https://localhost:5001",
      "ClientId": "cleancut-background-service",
  "ClientSecret": "BackgroundServiceSecret2024!",
      "Scope": "CleanCutAPI"
    },
    "Api": {
 "BaseUrl": "https://localhost:7142",
      "ProductsEndpoint": "/api/v1/products",
    "TimeoutSeconds": 30
    },
    "Csv": {
      "OutputDirectory": "exports",
      "FileNamePattern": "products_{0:yyyyMMdd_HHmmss}.csv",
      "IncludeHeaders": true,
    "KeepOldFiles": true,
 "MaxOldFiles": 10
    }
  }
}
```

## ?? IdentityServer Client Configuration

The service is registered in IdentityServer as:
- **Client ID**: `cleancut-background-service`
- **Grant Type**: Client Credentials
- **Scopes**: `CleanCutAPI`
- **Token Lifetime**: 1 hour

## ?? How to Run

### Prerequisites
1. **IdentityServer** running on `https://localhost:5001`
2. **CleanCut.API** running on `https://localhost:7142`
3. **Valid client credentials** configured in IdentityServer

### Running the Service

#### Option 1: Visual Studio
1. Set `CleanCut.BackgroundService` as startup project
2. Press F5 to run

#### Option 2: Command Line
```bash
cd src/Services/CleanCut.BackgroundService
dotnet run
```

#### Option 3: Published Executable
```bash
dotnet publish -c Release
cd bin/Release/net9.0/publish
dotnet CleanCut.BackgroundService.dll
```

## ?? What It Does

### Workflow
1. **Starts up** and logs configuration
2. **Waits 30 seconds** for other services to be ready
3. **Every 5 minutes** (configurable):
   - Authenticates with IdentityServer using client credentials
   - Calls CleanCut API to fetch all products
   - Exports products to CSV file with timestamp
   - Logs success/failure and file location
4. **Manages old files** (keeps last 10 by default)

### CSV Output
Files are saved to `/exports/` directory with format:
```
products_20241130_143022.csv
```

CSV contains:
- Product ID
- Product Name  
- Description
- Price (formatted)
- Status (Available/Unavailable)
- Customer ID
- Created Date

## ?? Services & Components

### Core Services
- **`ProductExportWorker`** - Main background service orchestrator
- **`AuthenticationService`** - Handles IdentityServer client credentials flow
- **`ApiService`** - Makes authenticated calls to CleanCut API
- **`CsvExportService`** - Exports data to CSV files

### Configuration
- **`BackgroundServiceSettings`** - Main configuration class
- **`IdentityServerSettings`** - Authentication configuration
- **`ApiSettings`** - API endpoint configuration
- **`CsvSettings`** - CSV export configuration

### Models
- **`ProductInfo`** - Product data transfer object

## ?? Logging

The service provides detailed logging:
- **Startup configuration** display
- **Authentication flow** status
- **API call results** and timing
- **CSV export** progress and file paths
- **Error handling** with context

### Log Levels
- `Information` - Normal operations and results
- `Debug` - Detailed flow information (CleanCut.BackgroundService namespace)
- `Warning` - Non-critical issues
- `Error` - Failures with full context

## ??? Development Features

### Error Handling
- **Authentication failures** - Clear messages about token issues
- **API failures** - HTTP status code and connectivity info
- **Timeout handling** - Configurable timeouts with proper cancellation
- **Graceful degradation** - Continues running after errors

### Security Features
- **Token caching** - Reuses valid tokens until near expiry
- **HTTPS validation** - Configurable certificate validation for development
- **Secret management** - Supports secure configuration in production
- **Authorization header cleanup** - Removes tokens after API calls

### Performance Features
- **Async operations** - Fully async/await throughout
- **Cancellation support** - Proper cancellation token usage
- **Memory efficient** - Streams CSV writing for large datasets
- **Configurable intervals** - Adjust frequency based on needs

## ?? Testing

### Manual Testing
1. Start IdentityServer and API
2. Run the background service
3. Check console output for authentication and API calls
4. Verify CSV files are created in `/exports/` directory
5. Check CSV content matches API data

### Integration Testing
The service can be tested with:
- Different IdentityServer configurations
- Various API response scenarios
- Network failure simulations
- Large product datasets

## ?? Dependencies

### Core Packages
- **Microsoft.Extensions.Hosting** (9.0.x) - Background service hosting
- **Microsoft.Extensions.Http** (9.0.x) - HTTP client factory
- **Duende.IdentityModel** (7.1.x) - OAuth2/OIDC client
- **CsvHelper** (33.1.x) - CSV reading/writing

### Framework
- **.NET 9.0** - Latest .NET with performance improvements

## ?? Production Deployment

### Configuration
- Store client secrets in secure configuration (Azure Key Vault, etc.)
- Use production-grade logging (Application Insights, Serilog)
- Configure proper HTTPS certificate validation
- Set appropriate intervals for production loads

### Monitoring
- Monitor CSV file creation and sizes
- Track authentication token renewal
- Monitor API response times and errors
- Set up alerts for service failures

### Scaling
- Run multiple instances with different intervals
- Use distributed scheduling for coordination
- Implement circuit breakers for resilience
- Add health checks for monitoring

## ?? Success Indicators

When running correctly, you should see:
```
info: CleanCut.BackgroundService.Workers.ProductExportWorker[0]
      Step 1: Authenticating with IdentityServer
info: CleanCut.BackgroundService.Services.AuthenticationService[0]
      Successfully obtained access token. Expires at: 2024-11-30 15:30:22
info: CleanCut.BackgroundService.Workers.ProductExportWorker[0]
      Step 2: Fetching products from CleanCut API
info: CleanCut.BackgroundService.Services.ApiService[0]
      Successfully fetched 15 products from API
info: CleanCut.BackgroundService.Workers.ProductExportWorker[0]
      Step 3: Exporting products to CSV
info: CleanCut.BackgroundService.Services.CsvExportService[0]
      Successfully exported 15 products to CSV file: D:\.NET STUDY\CleanCut\src\Services\CleanCut.BackgroundService\exports\products_20241130_143022.csv
info: CleanCut.BackgroundService.Workers.ProductExportWorker[0]
      Product export completed successfully in 00:00:02.1234567. CSV file saved to: exports\products_20241130_143022.csv
```

The CSV file will contain your product data and be ready for import into other systems or analysis tools.