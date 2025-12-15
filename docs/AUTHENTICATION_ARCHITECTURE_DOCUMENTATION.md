# ?? CleanCut Authentication & Authorization Documentation

## ??? **Architecture Overview**

The CleanCut application implements a comprehensive OAuth2/OpenID Connect authentication architecture using **Duende IdentityServer** as the central authentication authority. This enterprise-grade security implementation supports multiple client applications with different authentication patterns.

### **Authentication Flow Diagram**

```
???????????????????????    ???????????????????????    ???????????????????????
?   Client Apps      ?    ?   IdentityServer    ?    ?   CleanCut.API ?
?     ?    ?   (Authority)    ?    ?   (Resource Server) ?
???????????????????????    ???????????????????????    ???????????????????????
? • BlazorWebApp      ?????? • User Management   ?    ? • JWT Validation    ?
? • WebApp (MVC)   ?    ? • Token Issuance    ?    ? • Role-based AuthZ  ?
? • WinApp (Future)   ?    ? • Client AuthN      ?????? • Protected APIs    ?
? • Swagger UI   ?    ? • OAuth2/OIDC  ?    ? • Audit Logging ?
???????????????????????    ???????????????????????    ???????????????????????
```

---

## ?? **Core Authentication Components**

### **1. CleanCut.Infrastructure.Identity (IdentityServer)**

**Purpose**: Central OAuth2/OpenID Connect authentication server
**Port**: https://localhost:5001
**Technology**: Duende IdentityServer + ASP.NET Core Identity

#### **Key Files Documented**:
- `Config.cs` - Client and resource configuration
- `HostingExtensions.cs` - IdentityServer hosting and pipeline setup
- `SeedData.cs` - Test user creation with roles and claims

#### **Authentication Capabilities**:
- ? User authentication with enterprise password policies
- ? JWT token issuance with proper audience claims
- ? Client credentials validation
- ? Authorization code + PKCE flows
- ? Role-based claims (Admin/User)
- ? Account lockout protection
- ? Certificate-based token signing (production)

#### **Test Users Created**:
```csharp
// Admin User
Email: admin@cleancut.com
Role: Admin
Claims: Name, Department (IT), Employee ID (EMP001)

// Regular User  
Email: user@cleancut.com
Role: User
Claims: Name, Department (Sales), Employee ID (EMP002), Location
```

---

### **2. CleanCut.API (Resource Server)**

**Purpose**: Protected backend API serving business data
**Port**: https://localhost:7142
**Technology**: ASP.NET Core Web API with JWT Bearer authentication

#### **Key Files Documented**:
- `Program.cs` - JWT validation and security middleware configuration
- `ProductsController.cs` - Role-based authorization demonstration

#### **Security Features**:
- ? JWT Bearer token validation against IdentityServer
- ? Role-based authorization policies (AdminOnly, UserOrAdmin)
- ? Comprehensive input validation
- ? Rate limiting (100 req/min production, 1000 dev)
- ? Security headers (CSP, HSTS, XSS Protection)
- ? Audit logging without sensitive data exposure
- ? CORS restrictions to known client origins

#### **Authorization Policies**:
```csharp
// Available on all endpoints requiring UserOrAdmin:
• GET /api/v1/products
• POST /api/v1/products  
• PUT /api/v1/products/{id}
• GET /api/v1/products/{id}

// Restricted to AdminOnly:
• DELETE /api/v1/products/{id}
```

---

## ??? **Client Applications**

### **3. CleanCut.BlazorWebApp (Server-Side Client)**

**Purpose**: Blazor Server application with server-side authentication
**Port**: https://localhost:7297
**Authentication Flow**: Client Credentials (machine-to-machine)

#### **Key Files Documented**:
- `Program.cs` - Blazor Server configuration with data protection
- `Services/Auth/TokenService.cs` - OAuth2 Client Credentials implementation
- `Services/Auth/AuthenticatedHttpMessageHandler.cs` - Transparent token injection

#### **Authentication Pattern**:
```csharp
1. Application startup ? TokenService requests token from IdentityServer
2. Client credentials validated (CleanCutBlazorWebApp + secret)
3. JWT token cached with data protection encryption
4. AuthenticatedHttpMessageHandler adds Bearer token to all API requests
5. Blazor components receive authenticated data without auth complexity
```

#### **Security Benefits**:
- ? All tokens handled server-side (never exposed to browser)
- ? Data protection encryption for token storage
- ? Automatic token refresh before expiration
- ? SignalR connections secured with proper CSP
- ? No CORS issues (server-to-server communication)

---

### **4. CleanCut.WebApp (MVC User Authentication)**

**Purpose**: Traditional MVC web application with user authentication
**Port**: https://localhost:7144
**Authentication Flow**: Authorization Code + PKCE (user authentication)

#### **Key Files Documented**:
- `Program.cs` - OpenID Connect user authentication configuration
- `Services/Auth/TokenService.cs` - User token extraction and client credentials fallback

#### **Authentication Pattern**:
```csharp
1. User visits protected page ? Redirect to IdentityServer
2. User authenticates ? Authorization code returned with PKCE
3. MVC app exchanges code for ID token + Access token
4. Tokens stored in encrypted authentication cookie
5. API calls include user's access token automatically
6. Role-based UI and API access based on user claims
```

#### **Dual Authentication Modes**:
- ?? **User Tokens**: When user logged in, use their access token
- ?? **Client Credentials**: Fallback for system operations

---

### **5. CleanCut.WinApp (Desktop - Planned)**

**Purpose**: WinForms desktop application (future implementation)
**Authentication Flow**: Authorization Code + PKCE with system browser

#### **Key Files Documented**:
- `Program.cs` - Entry point with planned authentication architecture
- `Infrastructure/ServiceConfiguration.cs` - Service registration patterns

#### **Planned Authentication Pattern**:
```csharp
1. User clicks Login ? System browser opens to IdentityServer
2. User authenticates ? Browser redirected to localhost callback
3. App captures authorization code from callback URL
4. App exchanges code + PKCE verifier for tokens
5. Tokens stored with Windows Data Protection API
6. Desktop app makes authenticated API calls
```

#### **Security Considerations**:
- ?? Public client (no client secret) - relies on PKCE
- ?? Windows Data Protection API for local token storage
- ?? System browser for secure authentication
- ?? Deep linking for authentication callbacks

---

## ?? **Client Configuration Matrix**

| Client Application | Client ID | Grant Type | Scopes | User Auth | API Access |
|-------------------|-----------|------------|---------|-----------|-------------|
| **BlazorWebApp** | `CleanCutBlazorWebApp` | Client Credentials | `CleanCutAPI` | ? No | ? Yes (App-level) |
| **WebApp (MVC)** | `CleanCutWebApp` | Authorization Code + PKCE | `openid`, `profile`, `CleanCutAPI` | ? Yes | ? Yes (User-level) |
| **WinApp** | `CleanCutWinApp` | Authorization Code + PKCE | `openid`, `profile`, `CleanCutAPI` | ? Yes (Planned) | ? Yes (User-level) |
| **Swagger UI** | `swagger-ui` | Implicit | `openid`, `profile`, `CleanCutAPI` | ? Yes (Testing) | ? Yes (Testing) |
| **M2M Client** | `m2m.client` | Client Credentials | `CleanCutAPI` | ? No | ? Yes (System-level) |

---

## ??? **Security Features Implemented**

### **Enterprise Password Policies**
```csharp
• Minimum 12 characters
• Requires uppercase, lowercase, digits, and symbols
• Minimum 4 unique characters
• Account lockout: 5 attempts = 15 minute lockout
• Unique email requirements
• Email confirmation required
```

### **Token Security**
```csharp
• Certificate-based JWT signing in production
• 1-hour access token lifetime
• Audience claim validation (CleanCutAPI)
• Automatic token refresh with 5-minute buffer
• Data protection encryption for cached tokens
• No sensitive data in logs
```

### **API Security**
```csharp
• Rate limiting: 100 req/min (production), 1000 req/min (dev)
• Role-based authorization policies
• Comprehensive input validation
• Security headers: CSP, HSTS, X-Frame-Options
• CORS restrictions to known origins
• Audit logging with IP tracking
```

---

## ?? **Testing & Development**

### **Test Authentication**
```bash
# Start IdentityServer
cd src/Infrastructure/CleanCut.Infrastructure.Identity
dotnet run

# Start API
cd src/Presentation/CleanCut.API  
dotnet run

# Start Blazor App
cd src/Presentation/CleanCut.BlazorWebApp
dotnet run
```

### **Test Accounts**
```csharp
// Admin Access (can DELETE products)
Username: alice
Email: admin@cleancut.com
Role: Admin

// User Access (cannot DELETE products)  
Username: bob
Email: user@cleancut.com
Role: User
```

### **Token Testing**
```bash
# Get token via Swagger UI
https://localhost:7142/swagger

# Or use token helper page
https://localhost:7142/token-helper.html

# Test API with token
curl -H "Authorization: Bearer {token}" https://localhost:7142/api/v1/products
```

---

## ?? **Security Compliance**

| OWASP Top 10 2021 | Status | Implementation |
|-------------------|--------|----------------|
| **A01: Broken Access Control** | ? **SECURED** | Role-based auth, JWT validation |
| **A02: Cryptographic Failures** | ? **SECURED** | HTTPS, certificate signing, token encryption |
| **A03: Injection** | ? **SECURED** | Input validation, EF Core protection |
| **A04: Insecure Design** | ? **SECURED** | Rate limiting, secure architecture |
| **A05: Security Misconfiguration** | ? **SECURED** | Security headers, environment configs |
| **A07: Identity/Auth Failures** | ? **SECURED** | Strong passwords, account lockout |
| **A09: Security Logging Failures** | ? **SECURED** | Comprehensive audit logging |

---

## ?? **Production Deployment**

### **Environment Variables Required**
```bash
# IdentityServer
IDENTITY_SERVER_SIGNING_CERT_THUMBPRINT=your_certificate_thumbprint
IDENTITY_SERVER_URL=https://your-identity-server.com

# Client Secrets (Azure Key Vault)
BLAZOR_CLIENT_SECRET=your_secure_blazor_secret
WEBAPP_CLIENT_SECRET=your_secure_webapp_secret
M2M_CLIENT_SECRET=your_secure_m2m_secret

# Database Connections
API_DATABASE_CONNECTION_STRING=your_sql_connection_string
IDENTITY_DATABASE_CONNECTION_STRING=your_identity_sql_connection

# URLs
API_BASE_URL=https://your-api.com
BLAZOR_APP_URL=https://your-blazor-app.com
WEB_APP_URL=https://your-web-app.com
```

### **Security Checklist**
- [ ] SSL certificates installed and configured
- [ ] Azure Key Vault configured with all secrets
- [ ] Database encryption enabled
- [ ] Application Insights monitoring enabled
- [ ] Rate limiting policies configured
- [ ] CORS policies restricted to production origins
- [ ] Security headers validated
- [ ] Penetration testing completed

---

## ?? **Documentation Files Added**

Each file now contains comprehensive C# documentation headers explaining:

### **IdentityServer Files**:
- `Config.cs` - Client and resource configuration architecture
- `HostingExtensions.cs` - IdentityServer hosting and security setup
- `SeedData.cs` - Test user creation and role management

### **API Files**:
- `Program.cs` - JWT validation and security middleware
- `ProductsController.cs` - Role-based authorization demonstration

### **Blazor Server Files**:
- `Program.cs` - Server-side authentication configuration
- `TokenService.cs` - OAuth2 Client Credentials implementation
- `AuthenticatedHttpMessageHandler.cs` - Transparent token injection

### **MVC WebApp Files**:
- `Program.cs` - User authentication with OpenID Connect
- `TokenService.cs` - User token management and API access

### **WinApp Files**:
- `Program.cs` - Planned desktop authentication architecture
- `ServiceConfiguration.cs` - Service registration for authentication

---

**?? Result**: Every authentication-related file now contains detailed documentation explaining its role in the overall CleanCut authentication architecture, integration patterns, security features, and usage examples. This provides a comprehensive reference for understanding and maintaining the authentication system across all client applications.