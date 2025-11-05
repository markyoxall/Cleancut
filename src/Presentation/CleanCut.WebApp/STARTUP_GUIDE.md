# CleanCut MVC WebApp - OAuth 2.1 Startup Guide

## ?? **Overview**

The **CleanCut.WebApp** is a complete **ASP.NET Core MVC** application that demonstrates **OAuth 2.1/OpenID Connect** user authentication with secure API integration. This application serves as a **PUBLIC CLIENT** in the OAuth 2.1 authentication ecosystem, providing traditional web application patterns with modern security.

## ??? **OAuth 2.1 Authentication Architecture**

```mermaid
graph LR
  USER[User Browser] -->|HTTPS| WEBAPP[MVC WebApp<br/>Port 7286]
    WEBAPP -->|OAuth2.1<br/>Authorization Code + PKCE| IDENTITY[IdentityServer<br/>Port 5001]
    WEBAPP -->|Bearer Token<br/>API Calls| API[CleanCut.API<br/>Port 7142]
    
    subgraph "OAuth 2.1 Flow"
   LOGIN[Login Required]
  REDIRECT[Redirect to IdentityServer]
      AUTH[User Authentication + PKCE]
TOKENS[Access + ID Tokens]
 end
```

### ?? **OAuth 2.1 Client Configuration**
- **Client Type**: **PUBLIC CLIENT** (No client secrets)
- **Grant Type**: Authorization Code + PKCE (OAuth 2.1 compliant)
- **Client ID**: `CleanCutWebApp`
- **Client Secret**: ? **None** (Public client - PKCE provides security)
- **Scopes**: `openid`, `profile`, `CleanCutAPI`
- **Authentication**: User-driven OAuth 2.1 flow

## ?? **Required Startup Sequence**

> **?? Important**: All three services must be running for full functionality

### **Step 1: Start IdentityServer** (Port 5001)
```bash
cd "D:\.NET STUDY\CleanCut"
dotnet run --project src/Infrastructure/CleanCut.Infrastructure.Identity
```
**Verify**: Navigate to `https://localhost:5001` - should show IdentityServer welcome page

### **Step 2: Start API** (Port 7142)  
```bash
# In a new terminal
cd "D:\.NET STUDY\CleanCut"
dotnet run --project src/Presentation/CleanCut.API
```
**Verify**: Navigate to `https://localhost:7142/swagger` - should show API documentation

### **Step 3: Start MVC WebApp** (Port 7286)
```bash
# In a new terminal  
cd "D:\.NET STUDY\CleanCut"
dotnet run --project src/Presentation/CleanCut.WebApp
```
**Verify**: Navigate to `https://localhost:7286` - should show MVC application

## ?? **OAuth 2.1 Authentication Flow Testing**

### **User Authentication Process**
1. **Navigate to WebApp**: `https://localhost:7286`
2. **Click "Login"**: Redirects to IdentityServer login page
3. **Enter Credentials**: Use test accounts (see below)
4. **OAuth 2.1 PKCE Flow**: Automatic code exchange with PKCE verification
5. **Automatic Redirect**: Returns to WebApp with authenticated session
6. **Access Protected Pages**: Navigate to Products, Customers, etc.

### **?? Test Accounts**
| Role | Email | Password | Access Level |
|------|-------|----------|--------------|
| **Admin** | `admin@cleancut.com` | `TempPassword123!` | Full CRUD access (including delete) |
| **User** | `user@cleancut.com` | `TempPassword123!` | Create, Read, Update (no delete) |

## ??? **Available Features**

### **?? Home Dashboard**
- **Authentication Status**: Shows logged-in user information
- **User Claims**: Displays roles and permissions
- **API Connectivity**: Tests connection to protected API
- **Quick Links**: Navigation to main features

### **?? Customer Management** (`/Customers`)
- ? **View All Customers**: List all customers with pagination
- ? **Customer Details**: View detailed customer information
- ? **Create Customer**: Add new customers with validation
- ? **Edit Customer**: Update customer information
- ? **Delete Customer**: Remove customers (Admin only)
- ? **Search & Filter**: Find customers by name or email

### **?? Product Management** (`/Products`)
- ? **View All Products**: List products with customer assignment
- ? **Product Details**: View detailed product information  
- ? **Create Product**: Add new products linked to customers
- ? **Edit Product**: Update product details and pricing
- ? **Delete Product**: Remove products (Admin only)
- ? **Customer Filter**: View products by specific customer
- ? **Search Functionality**: Find products by name or description

### **?? Role-Based Access Control**
```razor
<!-- Admin-only features -->
@if (User.IsInRole("Admin"))
{
    <button class="btn btn-danger" onclick="deleteCustomer()">
        Delete Customer
    </button>
}
else
{
    <span class="text-muted">Admin access required</span>
}
```

## ??? **Technical Implementation**

### **?? OAuth 2.1 Authentication Configuration** (`Program.cs`)
```csharp
// ? OAuth 2.1 PUBLIC CLIENT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    // ? Enterprise security configuration
 options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.SameSite = SameSiteMode.Lax;
})
.AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
{
    options.Authority = "https://localhost:5001";
    options.ClientId = "CleanCutWebApp";
    // ? NO CLIENT SECRET - OAuth 2.1 Public Client
    options.ResponseType = "code";           // Authorization Code flow
options.UsePkce = true;      // ? PKCE required for OAuth 2.1
    options.SaveTokens = true;  // Store tokens for API calls
    options.GetClaimsFromUserInfoEndpoint = true;
  
    options.Scope.Clear();
    options.Scope.Add("openid");
    options.Scope.Add("profile");
    options.Scope.Add("CleanCutAPI");
});
```

### **?? API Integration**
```csharp
// Automatic Bearer token injection for USER tokens
public class AuthenticatedHttpMessageHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var httpContext = _httpContextAccessor.HttpContext;
  
        if (httpContext?.User.Identity?.IsAuthenticated == true)
        {
     // Get the user's access token from authentication session
        var accessToken = await httpContext.GetTokenAsync("access_token");
     
      if (!string.IsNullOrEmpty(accessToken))
         {
request.Headers.Authorization = 
           new AuthenticationHeaderValue("Bearer", accessToken);
    }
        }
   
 return await base.SendAsync(request, cancellationToken);
    }
}
```

### **?? Service Registration**
```csharp
// HTTP clients with USER authentication
builder.Services.AddHttpClient<ICustomerApiService, CustomerApiService>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7142");
})
.AddHttpMessageHandler<AuthenticatedHttpMessageHandler>();
```

## ?? **Testing Scenarios**

### **?? Authentication Testing**
1. **Login Flow**:
   - Visit any protected page
   - Should redirect to IdentityServer
   - Login with test credentials
   - Should return to original page authenticated

2. **Role-Based Access**:
   - Login as User ? Cannot see delete buttons
   - Login as Admin ? Can access all features
   - Attempt unauthorized action ? Shows access denied

3. **Token Management**:
   - Tokens automatically refreshed
   - Logout clears tokens and session
   - API calls include proper Bearer tokens

### **?? API Integration Testing**
1. **Customer Operations**:
   - Create ? Should save via API
   - Read ? Should fetch from API
   - Update ? Should modify via API
   - Delete ? Should remove via API (Admin only)

2. **Product Operations**:
   - All CRUD operations through authenticated API
   - Customer assignment validation
   - Price validation and formatting

3. **Error Scenarios**:
   - API unavailable ? Graceful error messages
   - Authentication expired ? Automatic re-authentication
   - Insufficient permissions ? Access denied page

## ? **Troubleshooting Guide**

### **?? Authentication Issues**

#### **"Unable to obtain configuration" Error**
```bash
# Check IdentityServer is running
curl -k https://localhost:5001/.well-known/openid_configuration

# Should return JSON configuration
```

#### **"Unauthorized" on API Calls**
1. Check access token in browser developer tools
2. Verify API is running and accessible
3. Check CORS configuration in API
4. Validate token in JWT.io

#### **Login Redirect Loop**
1. Check client configuration in IdentityServer
2. Verify redirect URIs match exactly  
3. Check cookie domain settings
4. Clear browser cache and cookies

### **??? API Integration Issues**

#### **No Data Displayed**
```bash
# Check API directly (requires user token, not client credentials)
# Use the web app's Profile page to get your user token
# Then test manually:
curl -H "Authorization: Bearer YOUR_USER_TOKEN" \
  https://localhost:7142/api/customers
```

#### **404 Not Found Errors**
1. Verify API routes in Swagger UI
2. Check base URL configuration
3. Ensure API controllers are properly registered

### **?? Common Configuration Issues**

#### **CORS Errors**
```json
// API appsettings.json - Should include MVC WebApp origin
{
  "AllowedOrigins": [
    "https://localhost:7286",  // ? MVC WebApp URL
    "https://localhost:7297",  // CleanCut.BlazorWebApp  
"https://localhost:7068"   // TempBlazorApp
  ]
}
```

#### **OAuth 2.1 Configuration**
```json
// WebApp appsettings.json - NO CLIENT SECRET NEEDED
{
  "IdentityServer": {
    "Authority": "https://localhost:5001",
    "ClientId": "CleanCutWebApp"
    // ? NO ClientSecret - OAuth 2.1 Public Client
  },
  "ApiClients": {
    "BaseUrl": "https://localhost:7142"
  }
}
```

## ?? **Diagnostic Commands**

### **Health Checks**
```bash
# IdentityServer health
curl -k https://localhost:5001/health

# API health  
curl -k https://localhost:7142/health

# Check WebApp health
curl -k https://localhost:7286/health
```

### **?? OAuth 2.1 Token Validation**
```bash
# Test OAuth 2.1 flow - Use authorization code flow (not client credentials)
# For testing user-specific API access:
# 1. Login via web app at https://localhost:7286
# 2. Navigate to /Account/Profile to view your tokens
# 3. Use the access token for API testing

# For service-to-service testing (different from user tokens):
curl -X POST "https://localhost:5001/connect/token" \
  -H "Content-Type: application/x-www-form-urlencoded" \
-d "grant_type=client_credentials&client_id=m2m.client&client_secret=511536EF-F270-4058-80CA-1C89C192F69A&scope=CleanCutAPI"
```

## ? **Success Indicators**

### **?? Fully Working OAuth 2.1 System**
- [ ] IdentityServer responds on port 5001
- [ ] API returns data on port 7142 (with authentication)
- [ ] WebApp loads on port 7286
- [ ] Login redirects to IdentityServer and back
- [ ] **OAuth 2.1 PKCE flow** completes successfully
- [ ] Protected pages require authentication
- [ ] Role-based features work correctly
- [ ] API integration shows live data
- [ ] Create/Edit operations save successfully
- [ ] Admin-only features hidden for regular users

### **?? Performance Metrics**
- **OAuth 2.1 Login Flow**: < 3 seconds total
- **Page Load**: < 2 seconds with authenticated API calls
- **API Response**: < 500ms for typical operations
- **Token Refresh**: Transparent to user

## ?? **Production Considerations**

### **?? OAuth 2.1 Security Hardening**
- Use proper SSL certificates
- **No client secrets to manage** (public client advantage)
- Configure proper CORS origins
- Implement rate limiting  
- Add comprehensive logging
- **PKCE provides security** without secret management complexity

### **?? Monitoring**
- OAuth 2.1 authentication success/failure rates
- PKCE flow completion metrics
- API response times
- Token usage patterns
- Error rates and types

---

**The MVC WebApp now provides a complete, secure, and production-ready example of OAuth 2.1 authentication with PKCE, demonstrating modern web application security patterns without the complexity of client secret management!** ??