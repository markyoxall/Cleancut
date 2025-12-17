using System.Collections.Generic;

namespace CleanCut.BlazorSPA.Pages.Weather.State
{
    public record WeatherState(bool IsLoading, IReadOnlyList<WeatherForecast> Forecasts, string? ErrorMessage)
    {
        public static WeatherState Initial => new WeatherState(false, Array.Empty<WeatherForecast>(), null);
    }
}
