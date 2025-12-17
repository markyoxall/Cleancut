using System.Collections.Generic;

namespace CleanCut.BlazorSPA.Pages.Weather.State
{
    public interface IWeatherSelectors
    {
        IReadOnlyList<WeatherForecast> WarmForecasts { get; }
        WeatherForecast? LatestForecast { get; }
    }
}
