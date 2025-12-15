using Duende.IdentityModel.Client;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace CleanCut.Infrastructure.BackgroundServices.Authentication;

/// <summary>
/// Service for handling IdentityServer authentication using client credentials flow
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    /// Gets an access token using client credentials
    /// </summary>
    Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Implementation of authentication service using Duende IdentityModel
/// </summary>
public class AuthenticationService : IAuthenticationService
{
    private readonly HttpClient _httpClient;
    private readonly AuthenticationSettings _settings;
    private readonly ILogger<AuthenticationService> _logger;
    
    // Cache token until it expires
    private string? _cachedToken;
    private DateTime _tokenExpiry = DateTime.MinValue;

    public AuthenticationService(
        HttpClient httpClient,
        IOptions<AuthenticationSettings> settings,
        ILogger<AuthenticationService> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
 }

    public async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        // Check if we have a valid cached token
      if (!string.IsNullOrEmpty(_cachedToken) && DateTime.UtcNow < _tokenExpiry.AddMinutes(-1))
    {
     _logger.LogDebug("Using cached access token");
   return _cachedToken;
}

        _logger.LogInformation("Requesting new access token from IdentityServer");

        try
        {
      string tokenEndpoint;
   
     if (_settings.TokenEndpoint != null)
            {
       // Use explicit token endpoint
         tokenEndpoint = _settings.TokenEndpoint;
            }
     else
    {
 // Discover endpoints
             var disco = await _httpClient.GetDiscoveryDocumentAsync(new DiscoveryDocumentRequest
 {
    Address = _settings.Authority,
          Policy = new DiscoveryPolicy
              {
       RequireHttps = _settings.Authority.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
          }
   }, cancellationToken);

  if (disco.IsError)
     {
    _logger.LogError("Failed to discover IdentityServer endpoints: {Error}", disco.Error);
        throw new InvalidOperationException($"Failed to discover IdentityServer endpoints: {disco.Error}");
  }
 
         tokenEndpoint = disco.TokenEndpoint ?? $"{_settings.Authority}/connect/token";
        }

          // Request token using client credentials
      var tokenResponse = await _httpClient.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = tokenEndpoint,
      ClientId = _settings.ClientId,
           ClientSecret = _settings.ClientSecret,
   Scope = _settings.Scope
            }, cancellationToken);

            if (tokenResponse.IsError)
    {
  _logger.LogError("Failed to obtain access token: {Error} - {ErrorDescription}", 
     tokenResponse.Error, tokenResponse.ErrorDescription);
          throw new InvalidOperationException($"Failed to obtain access token: {tokenResponse.Error}");
          }

            // Cache the token
   _cachedToken = tokenResponse.AccessToken;
        _tokenExpiry = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn);

            _logger.LogInformation("Successfully obtained access token. Expires at: {Expiry}", _tokenExpiry);
            return _cachedToken!;
   }
  catch (Exception ex) when (!(ex is InvalidOperationException))
        {
   _logger.LogError(ex, "Unexpected error while obtaining access token");
       throw new InvalidOperationException("Failed to obtain access token", ex);
        }
    }
}