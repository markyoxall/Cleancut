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