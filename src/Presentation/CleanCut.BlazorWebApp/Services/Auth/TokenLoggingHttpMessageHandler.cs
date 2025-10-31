using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace CleanCut.BlazorWebApp.Services.Auth;

public class TokenLoggingHttpMessageHandler : DelegatingHandler
{
    private readonly ILogger<TokenLoggingHttpMessageHandler> _logger;
   private readonly IServiceProvider _serviceProvider;

    public TokenLoggingHttpMessageHandler(
        ILogger<TokenLoggingHttpMessageHandler> logger,
      IServiceProvider serviceProvider)
    {
        _logger = logger;
    _serviceProvider = serviceProvider;
    }

   protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
      CancellationToken cancellationToken)
    {
    var callLog = new ApiCallLog
     {
 Timestamp = DateTime.UtcNow,
      Method = request.Method.ToString(),
     Url = request.RequestUri?.ToString() ?? "",
  RequestId = Guid.NewGuid().ToString("N")[..8]
        };

        // Extract token information
  var authHeader = request.Headers.Authorization;
        if (authHeader != null && authHeader.Scheme == "Bearer")
        {
     callLog.HasToken = true;
     callLog.TokenType = authHeader.Scheme;
         callLog.TokenPreview = authHeader.Parameter?.Length > 20 
     ? $"{authHeader.Parameter[..10]}...{authHeader.Parameter[^10..]}" 
        : authHeader.Parameter ?? "";
            callLog.TokenLength = authHeader.Parameter?.Length ?? 0;

            // Try to extract some basic info from the token
       try
  {
       if (!string.IsNullOrEmpty(authHeader.Parameter))
     {
          var parts = authHeader.Parameter.Split('.');
   if (parts.Length == 3)
       {
      // Try to decode the payload to get expiry info
         var payload = parts[1];
        // Add padding if needed
    var padding = payload.Length % 4;
        if (padding > 0)
              {
     payload += new string('=', 4 - padding);
       }

        var payloadBytes = Convert.FromBase64String(payload);
   var payloadJson = Encoding.UTF8.GetString(payloadBytes);
         
     using var doc = JsonDocument.Parse(payloadJson);
   var root = doc.RootElement;

     if (root.TryGetProperty("exp", out var expElement))
      {
           var exp = expElement.GetInt64();
          callLog.TokenExpiry = DateTimeOffset.FromUnixTimeSeconds(exp).DateTime;
    }

    if (root.TryGetProperty("aud", out var audElement))
    {
       if (audElement.ValueKind == JsonValueKind.Array)
         {
  var audiences = audElement.EnumerateArray()
        .Select(x => x.GetString())
       .Where(x => !string.IsNullOrEmpty(x))
    .ToArray();
        callLog.TokenAudience = string.Join(", ", audiences!);
          }
               else if (audElement.ValueKind == JsonValueKind.String)
                {
           callLog.TokenAudience = audElement.GetString() ?? "";
      }
      }

    if (root.TryGetProperty("client_id", out var clientElement))
       {
callLog.ClientId = clientElement.GetString() ?? "";
          }
     }
            }
       }
       catch (Exception ex)
 {
   _logger.LogDebug("Could not decode token info: {Error}", ex.Message);
            }
      }
   else
        {
 callLog.HasToken = false;
   }

        // Capture request headers
  callLog.RequestHeaders = string.Join(", ", 
  request.Headers.Select(h => $"{h.Key}: {string.Join(", ", h.Value)}"));

     var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
  {
            var response = await base.SendAsync(request, cancellationToken);
   
    stopwatch.Stop();
    callLog.Duration = stopwatch.Elapsed;
      callLog.StatusCode = (int)response.StatusCode;
        callLog.IsSuccess = response.IsSuccessStatusCode;
        callLog.ResponseHeaders = string.Join(", ", 
       response.Headers.Select(h => $"{h.Key}: {string.Join(", ", h.Value)}"));

     // If it's an auth failure, capture more details
   if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
          var wwwAuthHeader = response.Headers.WwwAuthenticate;
       if (wwwAuthHeader.Any())
              {
         callLog.AuthError = string.Join(", ", wwwAuthHeader.Select(h => h.ToString()));
    }
   }

      // Log to structured logging
    if (callLog.HasToken)
            {
         _logger.LogInformation(
   "API Call: {Method} {Url} -> {StatusCode} ({Duration}ms) [Token: {TokenLength} chars, Expires: {TokenExpiry}]",
          callLog.Method,
 callLog.Url,
  callLog.StatusCode,
      callLog.Duration.TotalMilliseconds,
         callLog.TokenLength,
         callLog.TokenExpiry?.ToString("yyyy-MM-dd HH:mm:ss UTC") ?? "Unknown");
            }
      else
            {
 _logger.LogInformation(
 "API Call: {Method} {Url} -> {StatusCode} ({Duration}ms) [No Token]",
            callLog.Method,
         callLog.Url,
callLog.StatusCode,
       callLog.Duration.TotalMilliseconds);
       }

   return response;
     }
      catch (Exception ex)
        {
     stopwatch.Stop();
            callLog.Duration = stopwatch.Elapsed;
            callLog.IsSuccess = false;
         callLog.Error = ex.Message;

         _logger.LogError(ex, 
         "API Call Failed: {Method} {Url} after {Duration}ms [Token: {HasToken}]",
       callLog.Method,
    callLog.Url,
            callLog.Duration.TotalMilliseconds,
        callLog.HasToken ? "Present" : "None");

       throw;
        }
        finally
        {
     // Add to log collection using the service
         try
         {
          using var scope = _serviceProvider.CreateScope();
var logService = scope.ServiceProvider.GetRequiredService<IApiCallLogService>();
 await logService.LogApiCallAsync(callLog);
      }
      catch (Exception ex)
      {
         _logger.LogError(ex, "Failed to log API call to service");
         }
        }
    }
}

public class ApiCallLog
{
    public DateTime Timestamp { get; set; }
    public string RequestId { get; set; } = "";
    public string Method { get; set; } = "";
    public string Url { get; set; } = "";
    public bool HasToken { get; set; }
    public string TokenType { get; set; } = "";
   public string TokenPreview { get; set; } = "";
    public int TokenLength { get; set; }
  public DateTime? TokenExpiry { get; set; }
  public string TokenAudience { get; set; } = "";
    public string ClientId { get; set; } = "";
  public TimeSpan Duration { get; set; }
    public int StatusCode { get; set; }
    public bool IsSuccess { get; set; }
    public string Error { get; set; } = "";
    public string AuthError { get; set; } = "";
    public string RequestHeaders { get; set; } = "";
  public string ResponseHeaders { get; set; } = "";
}