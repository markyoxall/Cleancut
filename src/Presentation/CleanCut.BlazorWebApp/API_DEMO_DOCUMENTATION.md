# CleanCut Blazor WebApp - Authenticated API Integration Demo

## Overview

The CleanCut Blazor WebApp features comprehensive **OAuth2/OIDC authenticated** API integration that demonstrates secure server-side API consumption with proper **JWT Bearer authentication**. The application showcases enterprise-level patterns for consuming protected APIs with automatic token management and role-based feature access.

## Authentication-Enabled Features

### **?? Secure API Integration**
- ? **Automatic JWT Token Injection** via `AuthenticatedHttpMessageHandler`
- ? **Role-based UI Rendering** based on user claims
- ? **Seamless Token Refresh** handling
- ? **Protected API Endpoints** requiring valid authentication
- ? **User Context Integration** with current user information

### **?? Interactive API Demonstration**
- ? **Live API Testing** with real authentication
- ? **Multiple API Versions** (V1 and V2) comparison
- ? **CRUD Operations** with proper authorization
- ? **Error Handling** for authentication and authorization failures
- ? **Performance Monitoring** with request timing

## Page Structure & Authentication Flow

### 1. **Authentication Required Pages**

#### **ServiceStatus.razor** (`/service-status`)
```csharp
@attribute [Authorize] // Requires authenticated user
```
- **Purpose**: Monitor API connectivity and authentication status
- **Features**:
  - IdentityServer connectivity testing
  - API authentication validation
  - JWT token inspection
  - Service health monitoring
  - User claims display

#### **ApiDemo.razor** (`/api-demo`)
```csharp
@attribute [Authorize] // Requires authenticated user
```
- **Purpose**: Comprehensive API testing interface with authentication
- **Features**:
  - Authenticated API endpoint testing
  - Role-based operation visibility
  - Real-time API call monitoring
  - JWT token usage demonstration

#### **ProductManagement.razor** (`/products`)
```csharp
@attribute [Authorize] // Requires authenticated user
```
- **Purpose**: Full product CRUD operations with authorization
- **Features**:
  - View all products (User/Admin access)
  - Create new products (User/Admin access)
  - Edit existing products (User/Admin access)
  - Delete products (Admin-only access)
  - Role-based UI elements

### 2. **API Integration Components**

#### **AuthTest.razor** (`/auth-test`)
- **Purpose**: Authentication flow testing and debugging
- **Features**:
  - User claims inspection
  - Token validation testing
  - API authentication testing
  - Login/logout functionality testing

## Authentication Architecture Integration

### **HTTP Client Configuration**
```csharp
// Program.cs - Automatic authentication setup
builder.Services.AddHttpClient<IProductApiClientV1>(client =>
{
  client.BaseAddress = new Uri("https://localhost:7142");
})
.AddHttpMessageHandler<AuthenticatedHttpMessageHandler>();
```

### **Authenticated HTTP Handler**
```csharp
public class AuthenticatedHttpMessageHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
    HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Automatically inject Bearer token from authentication context
        var accessToken = await _httpContextAccessor.HttpContext.GetTokenAsync("access_token");
      
        if (!string.IsNullOrEmpty(accessToken))
        {
     request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }
        
        return await base.SendAsync(request, cancellationToken);
    }
}
```

### **Role-Based API Services**
```csharp
public class ProductApiClientV1 : IProductApiClientV1
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ProductApiClientV1> _logger;

    public async Task<List<ProductInfo>> GetAllProductsAsync()
    {
        // HTTP client automatically includes JWT Bearer token
        var response = await _httpClient.GetAsync("/api/v1/products");
        
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
        throw new UnauthorizedAccessException("API access denied - invalid or expired token");
     }
        
  if (response.StatusCode == HttpStatusCode.Forbidden)
        {
        throw new UnauthorizedAccessException("API access denied - insufficient permissions");
        }
      
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
      return JsonSerializer.Deserialize<List<ProductInfo>>(json, _jsonOptions) ?? new List<ProductInfo>();
    }

    public async Task DeleteProductAsync(Guid id)
    {
        // This operation requires Admin role - enforced by API
        var response = await _httpClient.DeleteAsync($"/api/v1/products/{id}");
        
   if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            throw new UnauthorizedAccessException("Delete operation requires Admin role");
   }
   
        response.EnsureSuccessStatusCode();
    }
}
```

## Role-Based UI Implementation

### **Admin-Only Features**
```razor
<AuthorizeView Roles="Admin">
    <Authorized>
  <button class="btn btn-danger" @onclick="() => DeleteProduct(product.Id)">
            <i class="fas fa-trash"></i> Delete Product
        </button>
    </Authorized>
    <NotAuthorized>
  <span class="text-muted">
       <i class="fas fa-lock"></i> Admin access required
        </span>
    </NotAuthorized>
</AuthorizeView>
```

### **User Authentication Status**
```razor
<AuthorizeView>
    <Authorized>
        <div class="user-info">
      <h5>Welcome, @context.User.Identity.Name!</h5>
<p>Role: @string.Join(", ", context.User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value))</p>
            <p>Authentication: <span class="badge badge-success">Authenticated</span></p>
        </div>
    </Authorized>
    <NotAuthorized>
        <div class="alert alert-warning">
    <i class="fas fa-exclamation-triangle"></i>
      You must be logged in to access API features.
      <a href="/Account/Login" class="btn btn-primary btn-sm ml-2">Login</a>
        </div>
    </NotAuthorized>
</AuthorizeView>
```

## API Endpoints with Authentication

### **Products API V1** (Authentication Required)
| Method | Endpoint | Authorization | UI Access |
|--------|----------|---------------|-----------|
| GET | `/api/v1/products` | User/Admin | ? All authenticated users |
| GET | `/api/v1/products/{id}` | User/Admin | ? All authenticated users |
| GET | `/api/v1/products/customer/{customerId}` | User/Admin | ? All authenticated users |
| POST | `/api/v1/products` | User/Admin | ? All authenticated users |
| PUT | `/api/v1/products/{id}` | User/Admin | ? All authenticated users |
| DELETE | `/api/v1/products/{id}` | **Admin Only** | ?? Admin role required |

### **Products API V2** (Enhanced with Authentication)
| Method | Endpoint | Authorization | UI Features |
|--------|----------|---------------|-------------|
| GET | `/api/v2/products?page=1&pageSize=10` | User/Admin | Pagination controls |
| GET | `/api/v2/products/{id}` | User/Admin | Enhanced metadata display |
| GET | `/api/v2/products/statistics` | User/Admin | Admin dashboard widgets |

## Error Handling & Authentication

### **Authentication Error Scenarios**
```csharp
public async Task<List<ProductInfo>> HandleAuthenticatedApiCall()
{
    try
    {
        return await _productApiClient.GetAllProductsAsync();
    }
    catch (UnauthorizedAccessException ex)
    {
        // Handle authentication/authorization errors
_logger.LogWarning("API access denied: {Error}", ex.Message);
        
        // Redirect to login or show appropriate message
    await HandleAuthenticationFailure(ex.Message);
        return new List<ProductInfo>();
    }
    catch (HttpRequestException ex)
    {
        // Handle network/API errors
        _logger.LogError(ex, "API call failed");
   ShowErrorMessage("Unable to connect to API. Please try again.");
        return new List<ProductInfo>();
    }
}

private async Task HandleAuthenticationFailure(string error)
{
    if (error.Contains("invalid or expired token"))
{
     // Token expired - redirect to refresh
        _navigationManager.NavigateTo("/Account/Refresh", true);
    }
    else if (error.Contains("insufficient permissions"))
    {
        // Authorization error - show access denied
     ShowErrorMessage("You don't have permission to perform this action.");
    }
}
```

### **Real-Time Authentication Status**
```razor
<div class="authentication-status">
    @if (IsAuthenticated)
    {
        <div class="status-indicator status-success">
            <i class="fas fa-shield-alt"></i>
     <span>Authenticated</span>
            <small>Token expires: @TokenExpiration?.ToString("HH:mm")</small>
        </div>
    }
    else
    {
        <div class="status-indicator status-danger">
      <i class="fas fa-shield-alt"></i>
      <span>Not Authenticated</span>
        <a href="/Account/Login" class="btn btn-sm btn-primary">Login</a>
   </div>
    }
</div>
```

## Test Accounts for API Demo

### **Administrator Account**
```
Email: admin@cleancut.com
Password: TempPassword123!
Role: Admin
API Access: Full CRUD operations including DELETE
```

### **Regular User Account**
```
Email: user@cleancut.com
Password: TempPassword123!
Role: User
API Access: GET, POST, PUT operations (no DELETE)
```

## Usage Instructions

### **Starting the Complete Solution**
1. **Start IdentityServer**:
   ```bash
   dotnet run --project src/Infrastructure/CleanCut.Infrastructure.Identity
   # Available at: https://localhost:5001
   ```

2. **Start API**:
   ```bash
   dotnet run --project src/Presentation/CleanCut.API
   # Available at: https://localhost:7142
   ```

3. **Start Blazor App**:
   ```bash
   dotnet run --project src/Presentation/CleanCut.BlazorWebApp
   # Available at: https://localhost:7297
   ```

### **Testing the Authentication Flow**
1. **Navigate to Blazor App**: `https://localhost:7297`
2. **Click "Login"** to authenticate with IdentityServer
3. **Use test credentials** to log in
4. **Navigate to protected pages**:
   - `/service-status` - Check authentication and API connectivity
   - `/auth-test` - Inspect user claims and token information
   - `/api-demo` - Test authenticated API endpoints
   - `/products` - Full product management with role-based access

### **API Demo Features**
- **Service Status**: Monitor authentication status and API connectivity
- **API Testing**: Interactive testing of all authenticated endpoints
- **Role Demonstration**: See how Admin vs User roles affect available operations
- **Error Handling**: Experience authentication and authorization error scenarios

## Security Features Demonstrated

### **?? JWT Bearer Authentication**
- Automatic token injection in all API requests
- Token expiration handling and refresh
- Secure token storage in authentication cookies

### **??? Role-Based Authorization**
- UI elements shown/hidden based on user roles
- API endpoints protected by role requirements
- Graceful handling of authorization failures

### **?? PKCE Flow Integration**
- Authorization Code + PKCE flow for user authentication
- Secure redirect handling
- Proper logout and session management

### **?? Security Monitoring**
- Authentication event logging
- Failed authorization attempt tracking
- Token usage and expiration monitoring

---

**This Blazor WebApp demonstrates enterprise-level authenticated API integration, showcasing how modern web applications can securely consume protected APIs with OAuth2/OIDC authentication while providing excellent user experience and comprehensive error handling.**