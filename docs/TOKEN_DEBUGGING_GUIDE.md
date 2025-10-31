# Token Debugging Guide for CleanCut Blazor App

This guide explains how to see what tokens are being created and passed from your Blazor web app to your API.

## Available Tools

### 1. Token Inspector (`/token-inspector`)
**Purpose**: Examine the current JWT token in detail

**Features**:
- ?? **Token Analysis**: Decode and display JWT header, payload, and claims
- ? **Expiry Tracking**: See when tokens expire and time remaining
- ?? **Copy Token**: Copy the raw token to clipboard for external tools
- ?? **JWT.io Integration**: Open token directly in jwt.io for online debugging
- ?? **Claim Details**: View all token claims in a structured table

**Usage**:
1. Navigate to `/token-inspector` in your Blazor app
2. Click "Refresh Token" to get the current token
3. Examine the decoded token information
4. Use "Copy Token" or "Open in JWT.io" for further analysis

### 2. API Call Monitor (`/api-call-monitor`)
**Purpose**: Monitor HTTP requests to your API in real-time

**Features**:
- ?? **Real-time Monitoring**: See API calls as they happen
- ?? **Token Tracking**: See which calls include tokens and which don't
- ? **Performance Metrics**: Request duration and response times
- ?? **Auto-refresh**: Automatically update the call list
- ?? **Detailed Logs**: Full request/response headers and error information
- ?? **Status Tracking**: Visual indicators for successful/failed requests

**Usage**:
1. Navigate to `/api-call-monitor` in your Blazor app
2. Click "Start Auto-Refresh" to monitor calls in real-time
3. Make API calls from other pages (Products, Customers, etc.)
4. Watch the calls appear with token information
5. Click the info button on any call to see detailed information

### 3. Token Status (`/token-status`)
**Purpose**: Diagnostic information about the token service

**Features**:
- ?? **Service Diagnostics**: Test IdentityServer connectivity
- ?? **Endpoint Testing**: Verify discovery and token endpoints
- ?? **Configuration Check**: Validate IdentityServer settings
- ?? **Troubleshooting**: Detailed error information for authentication issues

## How Tokens Flow Through Your App

### 1. Token Creation
```
Blazor App ? TokenService ? IdentityServer ? JWT Token
```

- Your `TokenService` requests tokens using client credentials flow
- Tokens are cached and reused until they expire
- Each token includes audience, issuer, and expiry information

### 2. Token Usage
```
HTTP Request ? AuthenticatedHttpMessageHandler ? Add Bearer Token ? API
```

- Every API call goes through the `AuthenticatedHttpMessageHandler`
- The handler automatically adds the Bearer token to the Authorization header
- If no token is available, the request proceeds without authentication

### 3. Token Logging
```
HTTP Request ? TokenLoggingHttpMessageHandler ? Log Details ? AuthenticatedHttpMessageHandler
```

- The logging handler captures request details before adding the token
- It records token information, timing, and response status
- Logs are kept in memory for real-time monitoring

## Understanding Token Information

### Common Token Claims
- **`aud`** (Audience): Who the token is intended for (should include "CleanCutAPI")
- **`iss`** (Issuer): IdentityServer URL that issued the token
- **`exp`** (Expiry): Unix timestamp when token expires
- **`client_id`**: The client that requested the token
- **`scope`**: Permissions granted (should include "CleanCutAPI")

### Token Validation
Your API validates tokens by checking:
- **Issuer**: Must match your IdentityServer
- **Audience**: Must include "CleanCutAPI" or "https://localhost:5001/resources"
- **Expiry**: Token must not be expired
- **Signature**: Must be signed by IdentityServer

## Troubleshooting Common Issues

### Token Not Present
**Symptoms**: API calls show "No Token" in monitor
**Causes**:
- IdentityServer configuration missing
- Client credentials incorrect
- Network connectivity issues

**Solutions**:
1. Check `/token-status` for configuration issues
2. Verify IdentityServer is running
3. Check `appsettings.json` for correct settings

### Token Expired
**Symptoms**: 401 Unauthorized responses, expired tokens in monitor
**Causes**:
- System clock drift
- Long-running operations
- Token caching issues

**Solutions**:
1. Check token expiry time in `/token-inspector`
2. Tokens automatically refresh when expired
3. Check system time synchronization

### Wrong Audience
**Symptoms**: 401 Unauthorized despite having valid token
**Causes**:
- API expecting different audience
- Token issued for wrong scope

**Solutions**:
1. Check token audience in `/token-inspector`
2. Verify API configuration accepts your audience
3. Check scope in token request

### API Endpoint Issues
**Symptoms**: API calls fail with network errors
**Causes**:
- API not running
- Wrong base URL
- CORS issues

**Solutions**:
1. Check API base URL in configuration
2. Verify API is running on expected port
3. Check browser console for CORS errors

## Configuration Reference

### Blazor App Configuration (`appsettings.json`)
```json
{
  "IdentityServer": {
    "Authority": "https://localhost:5001",
    "ClientId": "blazor-client",
    "ClientSecret": "your-secret"
  },
  "ApiClients": {
    "BaseUrl": "https://localhost:7142"
  }
}
```

### API Configuration (Program.cs)
```csharp
options.TokenValidationParameters = new TokenValidationParameters
{
    ValidateIssuer = true,
    ValidateAudience = true,
    ValidateLifetime = true,
    ValidateIssuerSigningKey = true,
    ValidIssuer = identityServerAuthority,
    ValidAudiences = new[] { "CleanCutAPI", "https://localhost:5001/resources" }
};
```

## Best Practices

### Development
- Use `/api-call-monitor` to watch token usage in real-time
- Check `/token-inspector` when debugging authentication issues
- Keep `/token-status` open to monitor service health

### Production
- Monitor token expiry times
- Log authentication failures for analysis
- Implement proper error handling for token refresh failures
- Consider implementing token refresh before expiry

### Security
- Never log full tokens in production
- Use HTTPS for all token exchanges
- Rotate client secrets regularly
- Monitor for unusual token usage patterns

## Quick Start Checklist

1. ? **Start your services**:
   - IdentityServer running on port 5001
   - API running on port 7142
   - Blazor app running

2. ? **Test token service**:
   - Go to `/token-status` and click "Test Token Service"
   - Should see successful token request

3. ? **Monitor API calls**:
   - Go to `/api-call-monitor` and click "Start Auto-Refresh"
   - Navigate to Products or Customers pages
   - Watch API calls appear with token information

4. ? **Inspect tokens**:
   - Go to `/token-inspector` and click "Refresh Token"
   - Verify token has correct audience and expiry
   - Copy token to jwt.io for detailed analysis

You now have comprehensive visibility into your token flow from Blazor app to API!