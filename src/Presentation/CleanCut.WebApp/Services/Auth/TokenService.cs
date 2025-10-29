using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace CleanCut.WebApp.Services.Auth;

public interface ITokenService
{
    Task<string?> GetAccessTokenAsync();
}

public class TokenService : ITokenService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<TokenService> _logger;
    
    private string? _cachedToken;
    private DateTime _tokenExpiry = DateTime.MinValue;

 public TokenService(HttpClient httpClient, IConfiguration configuration, ILogger<TokenService> logger)
{
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
      
        // Configure HttpClient to handle SSL issues in development
      _httpClient.DefaultRequestHeaders.Add("User-Agent", "CleanCut.WebApp-TokenService");
    }

    public async Task<string?> GetAccessTokenAsync()
{
    // Return cached token if still valid (with 5 minute buffer)
        if (!string.IsNullOrEmpty(_cachedToken) && DateTime.UtcNow < _tokenExpiry.AddMinutes(-5))
        {
          _logger.LogDebug("Using cached access token");
   return _cachedToken;
        }

        try
        {
         _logger.LogInformation("Requesting new access token from IdentityServer");
   
         var authority = _configuration["IdentityServer:Authority"];
            var clientId = _configuration["IdentityServer:ClientId"];
    var clientSecret = _configuration["IdentityServer:ClientSecret"];

if (string.IsNullOrEmpty(authority) || string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
       {
   _logger.LogError("IdentityServer configuration is missing");
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
      _logger.LogError("Token request failed: {StatusCode} - {ErrorContent}", response.StatusCode, errorContent);
            return null;
 }

            var jsonContent = await response.Content.ReadAsStringAsync();
        _logger.LogDebug("Token response length: {Length}", jsonContent.Length);
     
   using var doc = JsonDocument.Parse(jsonContent);
       var root = doc.RootElement;

  if (root.TryGetProperty("access_token", out var tokenElement))
            {
       _cachedToken = tokenElement.GetString();
              
  // Get expiry time (default to 1 hour if not provided)
          var expiresIn = 3600; // Default 1 hour
              if (root.TryGetProperty("expires_in", out var expiryElement))
    {
            expiresIn = expiryElement.GetInt32();
    }

        _tokenExpiry = DateTime.UtcNow.AddSeconds(expiresIn);
      
        _logger.LogInformation("? Successfully obtained access token, expires in {ExpiresIn} seconds", expiresIn);
     return _cachedToken;
   }

      _logger.LogError("Token response did not contain access_token");
            return null;
        }
      catch (Exception ex)
{
            _logger.LogError(ex, "Error obtaining access token");
            return null;
   }
    }
}