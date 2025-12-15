/*
 * Authenticated HTTP Message Handler for MVC WebApp User Authentication
 * ====================================================================
 * 
 * This HTTP message handler automatically injects Bearer authentication tokens
 * from authenticated users into all outgoing HTTP requests to the CleanCut API.
 * It serves as the bridge between user authentication and API communication.
 * 
 * ROLE IN AUTHENTICATION ARCHITECTURE:
 * ------------------------------------
 * This handler acts as TRANSPARENT AUTHENTICATION middleware that:
 * 
 * 1. USER TOKEN INJECTION - Adds user's access token to API requests
 * 2. SEAMLESS INTEGRATION - API service classes don't need authentication logic
 * 3. CENTRALIZED AUTH - Single point for authentication across all HTTP clients
 * 4. USER CONTEXT - API calls are made on behalf of the authenticated user
 * 
 * USER AUTHENTICATION FLOW:
 * -------------------------
 * 1. User authenticates via OpenID Connect (Authorization Code + PKCE)
 * 2. Access token stored in authentication cookie/session
 * 3. MVC controller calls API service method
 * 4. This handler extracts user's access token from HTTP context
 * 5. Handler adds Authorization: Bearer {user_token} header
 * 6. API validates user token and returns user-specific data
 * 7. Response flows back to MVC controller with user context
 * 
 * This is the CORRECT approach for user-facing MVC applications!
 */

using Microsoft.AspNetCore.Authentication;
using System.Net.Http.Headers;

namespace CleanCut.WebApp.Services.Auth;

public class AuthenticatedHttpMessageHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;
 private readonly ILogger<AuthenticatedHttpMessageHandler> _logger;

 public AuthenticatedHttpMessageHandler(
   IHttpContextAccessor httpContextAccessor,
        ILogger<AuthenticatedHttpMessageHandler> logger)
    {
        _httpContextAccessor = httpContextAccessor;
    _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
  HttpRequestMessage request,
  CancellationToken cancellationToken)
    {
  try
{
        var httpContext = _httpContextAccessor.HttpContext;

        if (httpContext != null && httpContext.User.Identity?.IsAuthenticated == true)
        {
            // Get the user's access token from the authentication session
            var accessToken = await httpContext.GetTokenAsync("access_token");

            if (!string.IsNullOrEmpty(accessToken))
            {
                // Add the Bearer token to the request
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                _logger.LogDebug("Added user Bearer token to request: {Method} {Uri}", request.Method, request.RequestUri);
            }
            else
            {
                _logger.LogWarning("User is authenticated but no access token found in session");
            }
        }
        else
        {
            _logger.LogWarning("User not authenticated - API request will be sent without token");
        }
        }
  catch (Exception ex)
        {
 _logger.LogError(ex, "Error retrieving user access token for API request");
  }

     return await base.SendAsync(request, cancellationToken);
    }
}
