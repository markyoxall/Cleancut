namespace CleanCut.BlazorSPA.Pages.Weather
{
    public class WeatherForecast
    {
        // Use DateTime for simpler JSON deserialization in this demo
        public System.DateTime Date { get; set; }
        public int TemperatureC { get; set; }
        public string? Summary { get; set; }

        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
    }
}
