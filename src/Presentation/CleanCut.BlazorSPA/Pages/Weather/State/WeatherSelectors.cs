using System;
using System.Linq;
using System.Collections.Generic;
using Fluxor;
using CleanCut.BlazorSPA.Pages.Weather;

namespace CleanCut.BlazorSPA.Pages.Weather.State
{
    public static class WeatherSelectors
    {
        // Select forecasts warmer than 10°C (plain helper — no attribute/memoization in this Fluxor version)
        public static IReadOnlyList<WeatherForecast> GetWarmForecasts(WeatherState state) =>
            state.Forecasts?.Where(f => f.TemperatureC > 10).ToArray() ?? Array.Empty<WeatherForecast>();

        // Select the most recent forecast (or null)
        public static WeatherForecast? GetLatestForecast(WeatherState state) =>
            state.Forecasts?.OrderByDescending(f => f.Date).FirstOrDefault();
    }
}
