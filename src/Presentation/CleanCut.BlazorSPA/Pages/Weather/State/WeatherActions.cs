using System.Collections.Generic;

namespace CleanCut.BlazorSPA.Pages.Weather.State
{
    public record FetchWeatherAction;
    public record FetchWeatherSuccessAction(IReadOnlyList<WeatherForecast> Forecasts);
    public record FetchWeatherFailedAction(string ErrorMessage);
}
