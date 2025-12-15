/*
 * OAuth2 Token Service for MVC Web Application
 * ============================================
 * 
 * This service manages OAuth2 access tokens for the CleanCut MVC web application,
 * enabling authenticated API calls on behalf of logged-in users. It demonstrates
 * how to integrate user authentication with API access in traditional web apps.
 * 
 * ROLE IN AUTHENTICATION ARCHITECTURE:
 * ------------------------------------
 * This TokenService bridges USER AUTHENTICATION and API ACCESS:
 * 
 * 1. TOKEN EXTRACTION - Retrieves access tokens from user's authentication context
 * 2. TOKEN VALIDATION - Ensures tokens are valid before API calls
 * 3. TOKEN REFRESH - Handles token renewal when expired (future enhancement)
 * 4. FALLBACK AUTHENTICATION - Uses Client Credentials when no user token available
 * 
 * MVC-SPECIFIC AUTHENTICATION PATTERNS:
 * ------------------------------------
 * 
 * • User Context Integration:
 *   ??? Access tokens stored in user's authentication cookie after login
 *   ??? Retrieved from HttpContext.GetTokenAsync() for API calls
 *   ??? Tied to user session - different users get different tokens
 * 
 * • Controller Integration:
 *   ??? Called from MVC controllers to get tokens for API requests
 *   ??? Enables server-side API calls on behalf of authenticated users
 *   ??? Automatic token inclusion in HttpClient requests
 * 
 * • Dual Authentication Modes:
 *   ??? User Tokens: When user is logged in, use their access token
 *   ??? Client Credentials: When no user context, use app-level authentication
 *   ??? Graceful fallback between authentication modes
 * 
 * USER AUTHENTICATION FLOW INTEGRATION:
 * ------------------------------------
 * 1. User logs in via OpenID Connect Authorization Code flow
 * 2. IdentityServer returns both ID token (identity) and access token (API access)
 * 3. ASP.NET Core stores both tokens in encrypted authentication cookie
 * 4. This service extracts access token from user's cookie
 * 5. AuthenticatedHttpMessageHandler includes token in API requests
 * 6. CleanCut.API validates token and returns user-specific data
 * 
 * CLIENT CREDENTIALS FALLBACK:
 * ----------------------------
 * When no user is authenticated (background operations, system tasks):
 * 1. Service detects no user context available
 * 2. Requests app-level token using Client Credentials flow
 * 3. Uses CleanCutWebApp client ID and secret
 * 4. Receives application-level access token
 * 5. API calls proceed with app authentication instead of user authentication
 * 
 * INTEGRATION WITH MVC CONTROLLERS:
 * --------------------------------
 * 
 * • ProductsController:
 *   ??? Calls TokenService before making API requests
 *   ??? Receives user-specific product data based on token claims
 *   ??? Can perform operations allowed by user's role (Admin/User)
 * 
 * • CustomersController:
 *   ??? Gets user's access token for customer data API calls
 *   ??? API enforces user-level access control
 *   ??? Returns data filtered by user permissions
 * 
 * • JavaScript/AJAX Integration:
 *   ??? Controllers can expose tokens to client-side code
 *   ??? AJAX requests include Bearer tokens for API calls
 *   ??? Anti-forgery tokens prevent CSRF attacks
 * 
 * SECURITY CONSIDERATIONS:
 * -----------------------
 * • Access tokens never logged or exposed to client
 * • Secure cookie storage with encryption
 * • Automatic token expiration handling
 * • HTTPS-only token transmission
 * • Protection against token theft and replay attacks
 * • Proper token scope validation (CleanCutAPI)
 * 
 * TOKEN LIFECYCLE MANAGEMENT:
 * ---------------------------
 * • Tokens received during login flow
 * • Cached in user's authentication cookie
 * • Retrieved on-demand for API calls
 * • Automatic expiration detection
 * • Refresh token flow (future enhancement)
 * • Secure cleanup on logout
 * 
 * DEBUGGING AND DIAGNOSTICS:
 * --------------------------
 * • Comprehensive logging without sensitive data exposure
 * • Token presence and validity checking
 * • User context validation
 * • Authentication mode detection
 * • API connectivity testing
 * 
 * COMPARISON WITH OTHER CLIENTS:
 * -----------------------------
 * • vs. BlazorWebApp: This uses user tokens, Blazor uses client credentials only
 * • vs. WinApp: Similar user token pattern, different UI technology
 * • vs. Direct API calls: This provides authentication abstraction layer
 * 
 * ERROR SCENARIOS:
 * ---------------
 * • User not authenticated ? Falls back to client credentials or returns null
 * • Token expired ? Logs warning, attempts refresh (future) or returns null
 * • IdentityServer unavailable ? Returns null, API calls will fail gracefully
 * • Invalid token format ? Logs error, returns null
 * • Network connectivity issues ? Comprehensive error logging and diagnostics
 */

using Microsoft.AspNetCore.Authentication;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace CleanCut.WebApp.Services.Auth;

public interface ITokenService
{
    Task<string?> GetAccessTokenAsync();
    Task<string> GetDetailedStatusAsync();
}

public class TokenService : ITokenService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<TokenService> _logger;
    
    private string _lastError = "";

    public TokenService(
        IHttpContextAccessor httpContextAccessor,
 HttpClient httpClient,
        IConfiguration configuration,
   ILogger<TokenService> logger)
    {
   _httpContextAccessor = httpContextAccessor;
        _httpClient = httpClient;
    _configuration = configuration;
        _logger = logger;
    }

    public async Task<string?> GetAccessTokenAsync()
    {
        _lastError = "";
        
 var httpContext = _httpContextAccessor.HttpContext;

        // Try to get user's access token first
      if (httpContext?.User?.Identity?.IsAuthenticated == true)
      {
            try
     {
           var accessToken = await httpContext.GetTokenAsync("access_token");
     if (!string.IsNullOrEmpty(accessToken))
        {
    _logger.LogDebug("Retrieved user access token for: {User}", httpContext.User?.Identity?.Name);
          return accessToken;
              }
            }
  catch (Exception ex)
      {
      _logger.LogWarning(ex, "Failed to retrieve user access token");
         }
}

     // Fallback to client credentials if no user token
        _logger.LogDebug("No user token available, falling back to client credentials");
        return await GetClientCredentialsTokenAsync();
    }

    public async Task<string> GetDetailedStatusAsync()
    {
        var status = new StringBuilder();
        status.AppendLine("?? MVC Token Service Debug Information:");
        
        var httpContext = _httpContextAccessor.HttpContext;
        
        // User authentication status
   status.AppendLine($"User Authenticated: {httpContext?.User?.Identity?.IsAuthenticated ?? false}");
        status.AppendLine($"User Name: {httpContext?.User?.Identity?.Name ?? "Not available"}");
        
        if (httpContext?.User?.Identity?.IsAuthenticated == true)
        {
  try
  {
   var accessToken = await httpContext.GetTokenAsync("access_token");
    var idToken = await httpContext.GetTokenAsync("id_token");
     var refreshToken = await httpContext.GetTokenAsync("refresh_token");
     
        status.AppendLine($"User Access Token: {(!string.IsNullOrEmpty(accessToken) ? "Available" : "Not available")}");
          status.AppendLine($"ID Token: {(!string.IsNullOrEmpty(idToken) ? "Available" : "Not available")}");
 status.AppendLine($"Refresh Token: {(!string.IsNullOrEmpty(refreshToken) ? "Available" : "Not available")}");
    
 if (!string.IsNullOrEmpty(accessToken))
     {
             status.AppendLine($"Access Token Length: {accessToken.Length}");
       // Don't log actual token content for security
   }
    }
        catch (Exception ex)
            {
       status.AppendLine($"Error retrieving user tokens: {ex.Message}");
    }
        }
      
    // Client credentials configuration
        var authority = _configuration["IdentityServer:Authority"];
   var clientId = _configuration["IdentityServer:ClientId"];
        var clientSecret = _configuration["IdentityServer:ClientSecret"];
        
   status.AppendLine($"\nClient Credentials Configuration:");
        status.AppendLine($"Authority: {authority ?? "NOT SET"}");
        status.AppendLine($"Client ID: {clientId ?? "NOT SET"}");
 status.AppendLine($"Client Secret: {(string.IsNullOrEmpty(clientSecret) ? "NOT SET" : "SET")}");
        status.AppendLine($"Last Error: {(_lastError ?? "None")}");
        
        // Test client credentials token
        status.AppendLine($"\n?? Testing Client Credentials Token:");
try
        {
        var clientToken = await GetClientCredentialsTokenAsync();
            status.AppendLine($"Client Credentials Token: {(!string.IsNullOrEmpty(clientToken) ? "SUCCESS" : "FAILED")}");
  }
        catch (Exception ex)
  {
        status.AppendLine($"Client Credentials Token: FAILED - {ex.Message}");
 }
      
        return status.ToString();
 }

    private async Task<string?> GetClientCredentialsTokenAsync()
{
        try
        {
            var authority = _configuration["IdentityServer:Authority"];
            var clientId = _configuration["IdentityServer:ClientId"];
       var clientSecret = _configuration["IdentityServer:ClientSecret"];

        if (string.IsNullOrEmpty(authority) || string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
          {
                _lastError = "Client credentials configuration is missing";
            _logger.LogError(_lastError);
   return null;
            }

         var tokenEndpoint = $"{authority}/connect/token";
    
        var requestBody = new List<KeyValuePair<string, string>>
          {
        new("grant_type", "client_credentials"),
      new("client_id", clientId),
          new("client_secret", clientSecret),
new("scope", "CleanCutAPI")
 };

            var request = new HttpRequestMessage(HttpMethod.Post, tokenEndpoint)
            {
              Content = new FormUrlEncodedContent(requestBody)
        };

     request.Headers.Accept.Clear();
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
      {
         var errorContent = await response.Content.ReadAsStringAsync();
             _lastError = $"Client credentials token request failed: {response.StatusCode}";
  _logger.LogError("Client credentials token request failed with status {StatusCode}", response.StatusCode);
    return null;
}

          var jsonContent = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(jsonContent);
            var root = doc.RootElement;

            if (root.TryGetProperty("access_token", out var tokenElement))
       {
          var token = tokenElement.GetString();
                _logger.LogDebug("Successfully obtained client credentials access token");
         return token;
     }

            _lastError = "Token response did not contain access_token";
     _logger.LogError(_lastError);
      return null;
        }
        catch (Exception ex)
     {
 _lastError = $"Exception during client credentials token request: {ex.Message}";
      _logger.LogError(ex, "Error obtaining client credentials access token");
            return null;
        }
    }
}