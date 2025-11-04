# CleanCut Product Export Host

A .NET 9 application host that demonstrates **Clean Architecture** principles by using the `CleanCut.Infrastructure.BackgroundServices` layer to export product data.

## ??? Clean Architecture Structure

```
src/
??? Applications/
?   ??? CleanCut.ProductExportHost/  # ? Application Host
?       ??? Program.cs     # Composition root
?       ??? appsettings.json   # Configuration
?       ??? test-clean-architecture.ps1       # Test script
??? Infrastructure/
    ??? CleanCut.Infrastructure.BackgroundServices/  # ? Infrastructure Layer
        ??? Authentication/      # Identity Server integration
  ??? ExternalApi/           # CleanCut API integration
??? FileExport/    # CSV export services
        ??? ProductExport/   # Domain service orchestration
        ??? Models/               # DTOs
        ??? Extensions/     # Service registration
```

## ?? Clean Architecture Benefits

### ? **Separation of Concerns**
- **Application Layer**: Composition and hosting
- **Infrastructure Layer**: External system integrations
- **Each service has single responsibility**

### ? **Dependency Inversion**
- All dependencies are abstracted through interfaces
- Infrastructure depends on abstractions, not concretions
- Easy to test and mock

### ? **Reusability**
- Infrastructure services can be used by multiple applications
- Authentication service can be shared across background services
- CSV export service can export any data type

## ?? Running the Application

### Prerequisites
1. **IdentityServer** running on `https://localhost:5001`
2. **CleanCut.API** running on `https://localhost:7142`

### Quick Start
```bash
# From workspace root
cd src/Applications/CleanCut.ProductExportHost
dotnet run
```

### Using Test Script
```bash
# PowerShell
.\test-clean-architecture.ps1
```

## ?? Configuration

The application uses a clean configuration structure:

```json
{
  "Authentication": {
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
  },
  "ProductExport": {
    "IntervalMinutes": 5
  }
}
```

## ?? Architecture Components

### Infrastructure Layer Services

#### Authentication Service
- **Interface**: `IAuthenticationService`
- **Purpose**: Handle IdentityServer client credentials
- **Features**: Token caching, automatic discovery

#### API Service  
- **Interface**: `IApiService`
- **Purpose**: Call CleanCut API endpoints
- **Features**: Automatic token injection, error handling

#### CSV Export Service
- **Interface**: `ICsvExportService`  
- **Purpose**: Export data to CSV files
- **Features**: Configurable output, file cleanup

#### Product Export Service
- **Interface**: `IProductExportService`
- **Purpose**: Orchestrate the complete export workflow
- **Features**: Comprehensive logging, error handling

### Application Layer

#### ProductExportHost
- **Purpose**: Host the background service
- **Features**: Service composition, configuration management

## ?? Testing

The new Clean Architecture structure is easier to test:

```csharp
// Example unit test for ProductExportService
[Test]
public async Task ExportProductsAsync_WhenCalled_ShouldCompleteSuccessfully()
{
    // Arrange
    var mockAuth = new Mock<IAuthenticationService>();
    var mockApi = new Mock<IApiService>();
    var mockCsv = new Mock<ICsvExportService>();
    
    mockAuth.Setup(x => x.GetAccessTokenAsync(It.IsAny<CancellationToken>()))
     .ReturnsAsync("test-token");
    
    var service = new ProductExportService(
 mockAuth.Object, 
        mockApi.Object, 
        mockCsv.Object, 
        Mock.Of<ILogger<ProductExportService>>());
    
    // Act
    await service.ExportProductsAsync();
    
    // Assert
  mockAuth.Verify(x => x.GetAccessTokenAsync(It.IsAny<CancellationToken>()), Times.Once);
    mockApi.Verify(x => x.GetAllProductsAsync("test-token", It.IsAny<CancellationToken>()), Times.Once);
}
```

## ?? Migration Status

### ? **Completed**
- Infrastructure layer services created
- Application host configured
- Clean Architecture principles applied
- All dependencies properly abstracted
- Service registration through extensions

### ?? **Next Steps** (Optional)
1. Remove old `src/Services/CleanCut.BackgroundService/` folder
2. Update solution file to remove old project reference
3. Add unit tests for infrastructure services
4. Add integration tests for the complete workflow

## ?? **Architecture Compliance**

This implementation now properly follows:

### ? **Clean Architecture**
- **Infrastructure** contains external system integrations
- **Application** contains executable hosts
- **Dependencies flow inward** through interfaces

### ? **DDD Principles**
- **Domain services** orchestrate business workflows
- **Infrastructure services** handle technical concerns
- **Clear separation** between business and technical logic

### ? **SOLID Principles**
- **Single Responsibility**: Each service has one job
- **Open/Closed**: Services extensible through interfaces
- **Liskov Substitution**: All implementations follow contracts
- **Interface Segregation**: Focused, specific interfaces
- **Dependency Inversion**: Depends on abstractions

The new structure is production-ready and follows enterprise-level architecture patterns! ??