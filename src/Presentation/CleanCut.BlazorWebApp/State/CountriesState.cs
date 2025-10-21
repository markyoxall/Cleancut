using System.Threading;
using CleanCut.Application.DTOs;
using CleanCut.BlazorWebApp.Services;
using Microsoft.Extensions.Logging;

namespace CleanCut.BlazorWebApp.State;

public class CountriesState : ICountriesState
{
    private readonly ICountryApiService _countryApi;
    private readonly ILogger<CountriesState> _logger;

    private List<CountryInfo> _countries = new();
    private DateTime _lastLoaded = DateTime.MinValue;
    private readonly TimeSpan _cacheExpiry = TimeSpan.FromMinutes(5);

    private bool _isLoading;
    public bool IsLoading => _isLoading;
    public event Action<string, bool>? MessageChanged;

    public CountriesState(ICountryApiService countryApi, ILogger<CountriesState> logger)
    {
        _countryApi = countryApi;
        _logger = logger;
    }

    public IReadOnlyList<CountryInfo> Countries => _countries;
    public event Action? StateChanged;
    public event Action<List<CountryInfo>>? CountriesChanged;

    public async Task LoadAsync(bool force = false, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            throw new OperationCanceledException(cancellationToken);

        if (!force && DateTime.UtcNow - _lastLoaded < _cacheExpiry && _countries.Any())
        {
            _logger.LogDebug("CountriesState: using cached countries");
            return;
        }

        _isLoading = true;
        StateChanged?.Invoke();

        try
        {
            _logger.LogInformation("CountriesState: loading countries");
            _countries = await _countryApi.GetAllCountriesAsync(cancellationToken);
            _lastLoaded = DateTime.UtcNow;

            var copy = _countries.ToList();
            CountriesChanged?.Invoke(copy);
            StateChanged?.Invoke();

            _logger.LogInformation("CountriesState: loaded {Count} countries", _countries.Count);
            MessageChanged?.Invoke("Countries loaded", true);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("CountriesState: LoadAsync canceled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CountriesState: error loading countries");
            MessageChanged?.Invoke("Failed to load countries", false);
            // swallow to keep UI stable; callers can react to MessageChanged
        }
        finally
        {
            _isLoading = false;
            StateChanged?.Invoke();
        }
    }

    public void Invalidate()
    {
        _lastLoaded = DateTime.MinValue;
        StateChanged?.Invoke();
    }
}