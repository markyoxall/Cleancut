using Fluxor;

namespace CleanCut.BlazorSPA.Pages.Weather.State
{
    public class WeatherFeature : Feature<WeatherState>
    {
        public override string GetName() => "Weather";

        protected override WeatherState GetInitialState() => WeatherState.Initial;
    }
}
