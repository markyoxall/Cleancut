using Microsoft.AspNetCore.Authentication;
using System.Net.Http.Headers;

namespace CleanCut.BlazorWebApp.Services.Auth;

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
        var httpContext = _httpContextAccessor.HttpContext;
        
        if (httpContext?.User?.Identity?.IsAuthenticated == true)
        {
     try
            {
    // Get the access token from the authentication session
    var accessToken = await httpContext.GetTokenAsync("access_token");
             
  if (!string.IsNullOrEmpty(accessToken))
      {
   // Add the Bearer token to the request
      request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
  _logger.LogDebug("Added Bearer token to request: {Method} {Uri}", request.Method, request.RequestUri);
      }
    else
      {
       _logger.LogWarning("User is authenticated but no access token found");
   }
            }
      catch (Exception ex)
      {
       _logger.LogError(ex, "Error retrieving access token for API request");
         }
        }
        else
   {
  _logger.LogDebug("User not authenticated, sending request without token");
     }

        return await base.SendAsync(request, cancellationToken);
    }
}