using Fluxor;
using System;

namespace CleanCut.BlazorSPA.Pages.Weather.State
{
    public static class WeatherReducers
    {
        [ReducerMethod]
        public static WeatherState ReduceFetch(WeatherState state, FetchWeatherAction action) =>
            state with { IsLoading = true, ErrorMessage = null };

        [ReducerMethod]
        public static WeatherState ReduceFetchSuccess(WeatherState state, FetchWeatherSuccessAction action) =>
            state with { IsLoading = false, Forecasts = action.Forecasts };

        [ReducerMethod]
        public static WeatherState ReduceFetchFailed(WeatherState state, FetchWeatherFailedAction action) =>
            state with { IsLoading = false, ErrorMessage = action.ErrorMessage };
    }
}
