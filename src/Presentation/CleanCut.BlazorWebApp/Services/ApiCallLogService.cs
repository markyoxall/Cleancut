using CleanCut.BlazorWebApp.Services.Auth;

namespace CleanCut.BlazorWebApp.Services;

public interface IApiCallLogService
{
    Task<List<ApiCallLog>> GetRecentCallsAsync(int maxCount = 50);
    Task ClearLogsAsync();
    Task LogApiCallAsync(ApiCallLog callLog);
}

public class ApiCallLogService : IApiCallLogService
{
    private readonly List<ApiCallLog> _callLogs = new();
    private readonly SemaphoreSlim _logSemaphore = new(1, 1);
    private readonly ILogger<ApiCallLogService> _logger;

    public ApiCallLogService(ILogger<ApiCallLogService> logger)
    {
      _logger = logger;
    }

    public async Task<List<ApiCallLog>> GetRecentCallsAsync(int maxCount = 50)
    {
await _logSemaphore.WaitAsync();
        try
        {
            return _callLogs
.OrderByDescending(x => x.Timestamp)
    .Take(maxCount)
     .ToList();
      }
   finally
      {
    _logSemaphore.Release();
     }
    }

    public async Task ClearLogsAsync()
    {
        await _logSemaphore.WaitAsync();
    try
        {
        _callLogs.Clear();
     _logger.LogInformation("Cleared {Count} API call logs", _callLogs.Count);
        }
 finally
        {
  _logSemaphore.Release();
   }
    }

    public async Task LogApiCallAsync(ApiCallLog callLog)
    {
        await _logSemaphore.WaitAsync();
     try
        {
 _callLogs.Add(callLog);
  
 // Keep only the last 100 entries to prevent memory issues
       if (_callLogs.Count > 100)
            {
      _callLogs.RemoveRange(0, _callLogs.Count - 100);
            }

         _logger.LogDebug("Logged API call: {Method} {Url} -> {StatusCode}", 
           callLog.Method, callLog.Url, callLog.StatusCode);
        }
  finally
  {
_logSemaphore.Release();
  }
    }
}