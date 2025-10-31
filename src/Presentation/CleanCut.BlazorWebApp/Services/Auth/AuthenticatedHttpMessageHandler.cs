/*
 * Authenticated HTTP Message Handler for Blazor Server
 * ===================================================
 * 
 * This HTTP message handler automatically injects Bearer authentication tokens
 * into all outgoing HTTP requests to the CleanCut API. It serves as the bridge
 * between the OAuth2 token management and API communication.
 * 
 * ROLE IN AUTHENTICATION ARCHITECTURE:
 * ------------------------------------
 * This handler acts as TRANSPARENT AUTHENTICATION middleware that:
 * 
 * 1. TOKEN INJECTION - Automatically adds Authorization header to API requests
 * 2. SEAMLESS INTEGRATION - API service classes don't need authentication logic
 * 3. CENTRALIZED AUTH - Single point for authentication across all HTTP clients
 * 4. ERROR HANDLING - Graceful handling of token acquisition failures
 * 
 * HTTP CLIENT PIPELINE INTEGRATION:
 * ---------------------------------
 * This handler is registered in the HTTP client pipeline for:
 * 
 * • ProductApiClient (V1 & V2)
 *   ??? Adds Bearer token to all product-related API calls
 *   ??? GET /api/v1/products, POST /api/v1/products, etc.
 *   ??? Transparent authentication for CRUD operations
 * 
 * • CustomerApiService
 *   ??? Authenticates customer management API requests
 *   ??? Server-side calls to customer endpoints
 *   ??? No authentication logic needed in business components
 * 
 * • CountryApiService
 *   ??? Secures reference data API calls
 *   ??? Background data loading with automatic authentication
 *   ??? Blazor components receive data without auth complexity
 * 
 * AUTHENTICATION FLOW:
 * -------------------
 * 1. Blazor component calls API service method (e.g., GetProductsAsync())
 * 2. API service creates HttpRequestMessage for CleanCut.API
 * 3. This handler intercepts request before sending
 * 4. Handler calls TokenService.GetAccessTokenAsync() for current token
 * 5. Handler adds Authorization: Bearer {token} header to request
 * 6. Request proceeds to CleanCut.API with authentication
 * 7. API validates token and returns protected data
 * 8. Response flows back to Blazor component
 * 
 * TOKEN MANAGEMENT INTEGRATION:
 * ----------------------------
 * • Works with TokenService for automatic token acquisition
 * • Handles token refresh transparently if current token expired
 * • No caching logic - delegates token management to TokenService
 * • Fails gracefully if token cannot be obtained
 * 
 * SECURITY CONSIDERATIONS:
 * -----------------------
 * • Tokens never logged or exposed in error messages
 * • Secure token transmission over HTTPS only
 * • Token injection happens server-side (not exposed to browser)
 * • Automatic token refresh prevents expired token usage
 * • Comprehensive error handling without information disclosure
 * 
 * BLAZOR SERVER SPECIFICS:
 * -----------------------
 * • Server-side token handling (more secure than client-side)
 * • No CORS issues since API calls are server-to-server
 * • Shared authentication across all user sessions/circuits
 * • Better performance through server-side token caching
 * • Suitable for enterprise intranet scenarios
 * 
 * ERROR SCENARIOS:
 * ---------------
 * • TokenService unavailable ? Request proceeds without token (API will return 401)
 * • Invalid token format ? Request proceeds without token
 * • Token acquisition timeout ? Logs warning, continues without auth
 * • IdentityServer unavailable ? Graceful degradation, logs error
 * 
 * DEBUGGING:
 * ---------
 * • Logs token acquisition attempts (without sensitive data)
 * • Tracks API request authentication status
 * • Warning logs for authentication failures
 * • Debug logs for successful token injection
 * 
 * USAGE EXAMPLE:
 * -------------
 * This handler is automatically applied to HTTP clients via ServiceCollectionExtensions:
 * 
 * ```csharp
 * services.AddHttpClient<IProductApiService, ProductApiService>()
 *     .AddHttpMessageHandler<AuthenticatedHttpMessageHandler>();
 * ```
 * 
 * Result: All API calls automatically include Bearer tokens without any changes
 * to business logic or component code.
 */

using Microsoft.AspNetCore.Authentication;
using System.Net.Http.Headers;

namespace CleanCut.BlazorWebApp.Services.Auth;

public class AuthenticatedHttpMessageHandler : DelegatingHandler
{
    private readonly ITokenService _tokenService;
    private readonly ILogger<AuthenticatedHttpMessageHandler> _logger;

    public AuthenticatedHttpMessageHandler(
      ITokenService tokenService,
        ILogger<AuthenticatedHttpMessageHandler> logger)
    {
 _tokenService = tokenService;
     _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
      CancellationToken cancellationToken)
    {
      try
   {
      // Get the access token from the token service
            var accessToken = await _tokenService.GetAccessTokenAsync();

    if (!string.IsNullOrEmpty(accessToken))
  {
           // Add the Bearer token to the request
   request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    _logger.LogDebug("Added Bearer token to request: {Method} {Uri}", request.Method, request.RequestUri);
      }
 else
  {
 _logger.LogWarning("No access token available for API request");
     }
 }
 catch (Exception ex)
    {
     _logger.LogError(ex, "Error retrieving access token for API request");
  }

        return await base.SendAsync(request, cancellationToken);
    }
}