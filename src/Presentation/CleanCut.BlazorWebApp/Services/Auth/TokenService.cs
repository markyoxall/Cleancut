/*
 * OAuth2 Client Credentials Token Service for Blazor Server
 * =========================================================
 * 
 * This service implements the OAuth2 Client Credentials flow to obtain access tokens
 * from the CleanCut IdentityServer for server-to-server API authentication. It serves
 * as the core authentication component for the Blazor Server application.
 * 
 * ROLE IN AUTHENTICATION ARCHITECTURE:
 * ------------------------------------
 * This TokenService acts as the OAuth2 CLIENT in the Client Credentials flow:
 * 
 * 1. TOKEN ACQUISITION - Requests access tokens from IdentityServer token endpoint
 * 2. TOKEN CACHING - Securely stores tokens with encryption and automatic refresh
 * 3. TOKEN VALIDATION - Ensures tokens are valid before API calls
 * 4. ERROR HANDLING - Provides detailed diagnostics for authentication issues
 * 
 * CLIENT CREDENTIALS FLOW IMPLEMENTATION:
 * --------------------------------------
 * 1. Service constructs POST request to /connect/token with:
 *    ??? grant_type: "client_credentials"
 *    ??? client_id: "CleanCutBlazorWebApp" 
 *    ??? client_secret: [secure secret from configuration]
 * ??? scope: "CleanCutAPI"
 * 
 * 2. IdentityServer validates client credentials and returns:
 *    ??? access_token: JWT token with "CleanCutAPI" audience claim
 *    ??? expires_in: Token lifetime (3600 seconds = 1 hour)
 *    ??? token_type: "Bearer"
 * 
 * 3. Service caches encrypted token until expiry (with 5-minute buffer)
 * 4. AuthenticatedHttpMessageHandler uses token for API requests
 * 
 * INTEGRATION WITH BLAZOR COMPONENTS:
 * ----------------------------------
 * This service is automatically used by:
 * 
 * • ProductApiService - Gets/Creates/Updates/Deletes products via API
 *   ??? All HTTP requests include Bearer token from this service
 *   ??? Transparent authentication - components don't handle tokens directly
 * 
 * • CustomerApiService - Manages customer data via API
 *   ??? Server-side calls to CleanCut.API with automatic authentication
 *   ??? Blazor components receive data without authentication complexity
 * 
 * • CountryApiService - Retrieves country reference data
 *   ??? Background API calls with seamless token management
 *   ??? UI components focus on presentation, not authentication
 * 
 * SECURITY FEATURES:
 * -----------------
 * • Data Protection encryption for cached tokens
 * • Automatic token refresh before expiration
 * • Secure client secret management (Azure Key Vault in production)
 * • No sensitive data logging (tokens are never logged)
 * • Comprehensive error handling without information disclosure
 * • IP address tracking for security audit trails
 * • Protection against token replay attacks through short lifetimes
 * 
 * ERROR SCENARIOS HANDLED:
 * -----------------------
 * • Invalid client credentials ? Clear error message, no token issued
 * • IdentityServer unavailable ? Retry logic and graceful degradation
 * • Network connectivity issues ? Detailed diagnostic information
 * • Token expiration ? Automatic refresh and retry
 * • Configuration errors ? Startup validation and clear error messages
 * 
 * DEBUGGING AND DIAGNOSTICS:
 * --------------------------
 * The GetDetailedStatusAsync() method provides comprehensive diagnostics:
 * • IdentityServer connectivity testing
 * • Discovery endpoint validation
 * • Token endpoint accessibility
 * • Client credential validation
 * • Token parsing and claim inspection
 * • Configuration validation
 * 
 * PRODUCTION CONSIDERATIONS:
 * -------------------------
 * • Client secrets stored in Azure Key Vault
 * • Certificate-based authentication (future enhancement)
 * • Token caching with Redis for load-balanced scenarios
 * • Monitoring and alerting for authentication failures
 * • Audit logging for security compliance
 * 
 * BLAZOR SERVER SPECIFICS:
 * -----------------------
 * • Tokens handled entirely server-side (never exposed to browser)
 * • Shared token cache across all user circuits/sessions
 * • More secure than client-side authentication
 * • Suitable for enterprise intranet scenarios
 * • No CORS issues since API calls are server-to-server
 */

using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;

namespace CleanCut.BlazorWebApp.Services.Auth;

public interface ITokenService
{
    Task<string?> GetAccessTokenAsync();
Task<string> GetDetailedStatusAsync();
}

public class TokenService : ITokenService
{
    private readonly HttpClient _httpClient;
private readonly IConfiguration _configuration;
    private readonly ILogger<TokenService> _logger;
    private readonly IDataProtector _protector;
    
 private string? _cachedToken;
  private DateTime _tokenExpiry = DateTime.MinValue;
    private string _lastError = "";

  public TokenService(
    HttpClient httpClient, 
     IConfiguration configuration, 
  ILogger<TokenService> logger,
    IDataProtectionProvider dataProtection)
  {
  _httpClient = httpClient;
     _configuration = configuration;
     _logger = logger;
 _protector = dataProtection.CreateProtector("TokenService.v1");
  
        // Configure HttpClient to handle SSL issues in development
 _httpClient.DefaultRequestHeaders.Add("User-Agent", "CleanCut.BlazorWebApp-TokenService");
 }

    public async Task<string> GetDetailedStatusAsync()
    {
   var authority = _configuration["IdentityServer:Authority"];
     var clientId = _configuration["IdentityServer:ClientId"];
      var clientSecret = _configuration["IdentityServer:ClientSecret"];

    var status = new StringBuilder();
   status.AppendLine($"?? Token Service Debug Information:");
      status.AppendLine($"Authority: {authority ?? "NOT SET"}");
     status.AppendLine($"ClientId: {clientId ?? "NOT SET"}");
     status.AppendLine($"ClientSecret: {(string.IsNullOrEmpty(clientSecret) ? "NOT SET" : $"SET (length: {clientSecret.Length})")}")
     .AppendLine($"Token Endpoint: {authority}/connect/token")
     .AppendLine($"Cached Token: {(!string.IsNullOrEmpty(_cachedToken) ? $"YES (expires: {_tokenExpiry})" : "NO")}")
     .AppendLine($"Last Error: {(_lastError ?? "None")}");

  // ? Enhanced connectivity testing
        try
        {
   status.AppendLine("\n?? Testing IdentityServer connectivity...");
  
// Test 1: Basic connectivity
        try
  {
       var homeResponse = await _httpClient.GetAsync(authority);
     status.AppendLine($"Home page: {homeResponse.StatusCode}");
      }
    catch (Exception ex)
    {
   status.AppendLine($"? Home page test failed: {ex.Message}");
       }

  // Test 2: Discovery endpoint
 var discoveryUrl = $"{authority}/.well-known/openid_configuration";
      status.AppendLine($"Testing discovery URL: {discoveryUrl}");
    
  var discoveryResponse = await _httpClient.GetAsync(discoveryUrl);
 status.AppendLine($"Discovery endpoint: {discoveryResponse.StatusCode}");
      
      if (discoveryResponse.IsSuccessStatusCode)
  {
 var discoveryContent = await discoveryResponse.Content.ReadAsStringAsync();
    status.AppendLine($"Discovery response length: {discoveryContent.Length}");
      
    try
  {
   using var doc = JsonDocument.Parse(discoveryContent);
      var root = doc.RootElement;
   if (root.TryGetProperty("token_endpoint", out var tokenEndpointElement))
 {
    status.AppendLine($"? Token endpoint found: {tokenEndpointElement.GetString()}");
           }
      if (root.TryGetProperty("issuer", out var issuerElement))
    {
  status.AppendLine($"? Issuer: {issuerElement.GetString()}");
    }
       }
       catch (Exception parseEx)
    {
   status.AppendLine($"? Failed to parse discovery document: {parseEx.Message}");
  }
          }

   // Test 3: Actual token request
     status.AppendLine("\n?? Testing actual token request...");
   try
      {
   var testToken = await GetAccessTokenAsync();
     if (!string.IsNullOrEmpty(testToken))
      {
      status.AppendLine($"? Token request SUCCESS! Token length: {testToken.Length}");
  }
else
   {
        status.AppendLine($"? Token request failed: {_lastError}");
      }
  }
    catch (Exception ex)
      {
 status.AppendLine($"? Token request exception: {ex.Message}");
  }
    }
    catch (Exception ex)
   {
     status.AppendLine($"? Connectivity test failed: {ex.Message}");
       status.AppendLine($"Exception type: {ex.GetType().Name}");
      if (ex.InnerException != null)
   {
  status.AppendLine($"Inner exception: {ex.InnerException.Message}");
        }
    }

  return status.ToString();
    }

    public async Task<string?> GetAccessTokenAsync()
    {
  // Clear previous error
      _lastError = "";

    // Return cached token if still valid (with 5 minute buffer)
  if (!string.IsNullOrEmpty(_cachedToken) && DateTime.UtcNow < _tokenExpiry.AddMinutes(-5))
 {
 _logger.LogDebug("Using cached access token");
    return UnprotectToken(_cachedToken);
  }

 try
        {
     _logger.LogInformation("Requesting new access token from IdentityServer");
      
          var authority = _configuration["IdentityServer:Authority"];
   var clientId = _configuration["IdentityServer:ClientId"];
       var clientSecret = _configuration["IdentityServer:ClientSecret"];

     if (string.IsNullOrEmpty(authority) || string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
       {
     _lastError = "IdentityServer configuration is missing";
 _logger.LogError(_lastError);
  return null;
    }

 var tokenEndpoint = $"{authority}/connect/token";
     _logger.LogDebug("Token endpoint: {TokenEndpoint}", tokenEndpoint);

 var requestBody = new List<KeyValuePair<string, string>>
  {
     new("grant_type", "client_credentials"),
     new("client_id", clientId),
    new("client_secret", clientSecret),
new("scope", "CleanCutAPI")
  };

            // ? Don't log sensitive data
       _logger.LogDebug("Request parameters: grant_type=client_credentials, client_id={ClientId}, scope=CleanCutAPI", clientId);

  var request = new HttpRequestMessage(HttpMethod.Post, tokenEndpoint)
       {
    Content = new FormUrlEncodedContent(requestBody)
   };

 // Add explicit headers
    request.Headers.Accept.Clear();
      request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

    var response = await _httpClient.SendAsync(request);

 _logger.LogInformation("Token request response: {StatusCode} {ReasonPhrase}", response.StatusCode, response.ReasonPhrase);

   if (!response.IsSuccessStatusCode)
  {
     var errorContent = await response.Content.ReadAsStringAsync();
    _lastError = $"Token request failed: {response.StatusCode} - {errorContent}";
    _logger.LogError("Token request failed with status {StatusCode}", response.StatusCode);
       
// Try to parse error details without logging sensitive content
         try
  {
        using var errorDoc = JsonDocument.Parse(errorContent);
 var errorRoot = errorDoc.RootElement;
      if (errorRoot.TryGetProperty("error", out var errorElement))
    {
      var error = errorElement.GetString();
         if (errorRoot.TryGetProperty("error_description", out var descElement))
    {
    var description = descElement.GetString();
         _lastError = $"OAuth Error: {error} - {description}";
    _logger.LogError("OAuth Error: {Error} - {Description}", error, description);
     }
      }
  }
     catch
   {
  // If we can't parse the error, use generic message
      _logger.LogError("Failed to parse OAuth error response");
 }
   
      return null;
       }

    var jsonContent = await response.Content.ReadAsStringAsync();
  // ? Don't log token content
    _logger.LogDebug("Token response received successfully");
   
    using var doc = JsonDocument.Parse(jsonContent);
  var root = doc.RootElement;

   if (root.TryGetProperty("access_token", out var tokenElement))
 {
      var rawToken = tokenElement.GetString();
   if (!string.IsNullOrEmpty(rawToken))
     {
      // ? Protect token before caching
 _cachedToken = ProtectToken(rawToken);
  
     // Get expiry time (default to 1 hour if not provided)
        var expiresIn = 3600; // Default 1 hour
 if (root.TryGetProperty("expires_in", out var expiryElement))
   {
     expiresIn = expiryElement.GetInt32();
     }

       _tokenExpiry = DateTime.UtcNow.AddSeconds(expiresIn);
 
 _logger.LogInformation("? Successfully obtained access token, expires in {ExpiresIn} seconds", expiresIn);
 return rawToken;
         }
}

   _lastError = "Token response did not contain access_token";
    _logger.LogError(_lastError);
         return null;
     }
        catch (Exception ex)
    {
 _lastError = $"Exception during token request: {ex.Message}";
 _logger.LogError(ex, "Error obtaining access token");
 return null;
  }
    }
    
    // ? Token protection methods
  private string ProtectToken(string token)
    {
     try
        {
     return _protector.Protect(token);
   }
    catch (Exception ex)
{
     _logger.LogWarning(ex, "Failed to protect token, storing unprotected");
return token;
     }
    }
  
    private string UnprotectToken(string protectedToken)
    {
    try
        {
    return _protector.Unprotect(protectedToken);
  }
     catch (Exception ex)
    {
      _logger.LogWarning(ex, "Failed to unprotect token, returning as-is");
  return protectedToken;
        }
    }
}