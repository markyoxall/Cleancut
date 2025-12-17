using Fluxor;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace CleanCut.BlazorSPA.Pages.Weather.State
{
    public class WeatherEffects
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<WeatherEffects> _logger;

        public WeatherEffects(HttpClient httpClient, ILogger<WeatherEffects> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        [EffectMethod]
        public async Task HandleFetch(FetchWeatherAction action, IDispatcher dispatcher)
        {
            try
            {
                var forecasts = await _httpClient.GetFromJsonAsync<WeatherForecast[]>("weatherforecast.json");
                dispatcher.Dispatch(new FetchWeatherSuccessAction(forecasts ?? new WeatherForecast[0]));
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Failed to load weather forecasts");
                dispatcher.Dispatch(new FetchWeatherFailedAction(ex.Message));
            }
        }
    }
}
