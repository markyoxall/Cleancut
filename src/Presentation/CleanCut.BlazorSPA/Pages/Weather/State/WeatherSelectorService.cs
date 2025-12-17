using System;
using System.Collections.Generic;
using System.Linq;
using Fluxor;

namespace CleanCut.BlazorSPA.Pages.Weather.State
{
    // Service that computes selector values and caches them.
    public class WeatherSelectorService : IWeatherSelectors, IDisposable
    {
        private readonly IState<WeatherState> _state;

        private IReadOnlyList<WeatherForecast> _warmForecasts = Array.Empty<WeatherForecast>();
        private WeatherForecast? _latestForecast;

        public WeatherSelectorService(IState<WeatherState> state)
        {
            _state = state;
            ComputeSelectors(_state.Value);
            _state.StateChanged += OnStateChanged;
        }

        public IReadOnlyList<WeatherForecast> WarmForecasts => _warmForecasts;
        public WeatherForecast? LatestForecast => _latestForecast;

        private void OnStateChanged(object? sender, EventArgs e)
        {
            ComputeSelectors(_state.Value);
        }

        private void ComputeSelectors(WeatherState state)
        {
            var forecasts = state.Forecasts ?? Array.Empty<WeatherForecast>();
            var warm = forecasts.Where(f => f.TemperatureC > 10).ToArray();
            _warmForecasts = warm;
            _latestForecast = forecasts.OrderByDescending(f => f.Date).FirstOrDefault();
        }

        public void Dispose()
        {
            _state.StateChanged -= OnStateChanged;
        }
    }
}
