using System.Net.Http.Headers;

namespace CleanCut.WebApp.Services.Auth;

public class AuthenticatedHttpMessageHandler : DelegatingHandler
{
  private readonly ITokenService _tokenService;
   private readonly ILogger<AuthenticatedHttpMessageHandler> _logger;

   public AuthenticatedHttpMessageHandler(ITokenService tokenService, ILogger<AuthenticatedHttpMessageHandler> logger)
   {
  _tokenService = tokenService;
       _logger = logger;
   }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try
        {
     // Get access token
      var token = await _tokenService.GetAccessTokenAsync();
 
  if (!string.IsNullOrEmpty(token))
         {
// Add Bearer token to request
           request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
_logger.LogDebug("Added Bearer token to request for {Method} {Uri}", request.Method, request.RequestUri);
    }
     else
    {
    _logger.LogWarning("No access token available for request to {Uri}", request.RequestUri);
     }
        }
  catch (Exception ex)
        {
   _logger.LogError(ex, "Error adding authentication to request");
}

   return await base.SendAsync(request, cancellationToken);
    }
}