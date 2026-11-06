namespace WeatherAPI.App.Abstractions
{
    public interface IWeatherService
    {
        Task<WeatherObservation?> GetCurrentAsync(string cityName, string countryCode, CancellationToken cancellationToken);
    }

    public class WeatherObservation
    {
        public string CityName { get; set; } = string.Empty;
        public string CountryCode { get; set; } = string.Empty;
        public DateTime ObservedAtUtc { get; set; }
        public double TemperatureFahrenheit { get; set; }
        public int HumidityPercent { get; set; }
        public int PressureHpa { get; set; }
        public int? VisibilityMeters { get; set; }
        public double WindSpeedMph { get; set; }
        public int? WindDirectionDegrees { get; set; }
        public string SkyCondition { get; set; } = string.Empty;
        public string SkyDescription { get; set; } = string.Empty;
    }
}
