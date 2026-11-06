using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http.Json;
using WeatherAPI.App.Abstractions;

namespace WeatherAPI.Infra
{
    public class OpenWeatherMapOptions
    {
        public const string SectionName = "OpenWeatherMap";

        public string BaseUrl { get; set; } = "https://api.openweathermap.org/";
        public string ApiKey { get; set; } = string.Empty;
    }

    public class OpenWeatherMapService(HttpClient httpClient, IOptions<OpenWeatherMapOptions> options) : IWeatherService
    {
        public async Task<WeatherObservation?> GetCurrentAsync(string cityName, string countryCode, CancellationToken cancellationToken)
        {
            var apiKey = options.Value.ApiKey;
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new InvalidOperationException("OpenWeatherMap API key is not configured. Set 'OpenWeatherMap:ApiKey' via user secrets or environment variable.");

            var location = Uri.EscapeDataString($"{cityName},{countryCode}");
            var url = $"data/2.5/weather?q={location}&units=imperial&appid={Uri.EscapeDataString(apiKey)}";

            using var response = await httpClient.GetAsync(url, cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            response.EnsureSuccessStatusCode();

            var payload = await response.Content.ReadFromJsonAsync<OwmResponse>(cancellationToken) ?? throw new InvalidOperationException("OpenWeatherMap returned an empty response.");

            return Map(payload);
        }

        private static WeatherObservation Map(OwmResponse payload)
        {
            var sky = payload.Weather.FirstOrDefault();

            return new WeatherObservation
            {
                CityName = payload.Name,
                CountryCode = payload.Sys?.Country ?? string.Empty,
                ObservedAtUtc = DateTimeOffset.FromUnixTimeSeconds(payload.Dt).UtcDateTime,
                TemperatureFahrenheit = payload.Main.Temp,
                HumidityPercent = payload.Main.Humidity,
                PressureHpa = payload.Main.Pressure,
                VisibilityMeters = payload.Visibility,
                WindSpeedMph = payload.Wind?.Speed ?? 0,
                WindDirectionDegrees = payload.Wind?.Deg,
                SkyCondition = sky?.Main ?? "Unknown",
                SkyDescription = sky?.Description ?? string.Empty
            };
        }
    }

    // ---- OpenWeatherMap JSON contract (internal: never leaves the Infra layer) ----

    internal class OwmResponse
    {
        public string Name { get; set; } = string.Empty;
        public long Dt { get; set; }
        public int? Visibility { get; set; }
        public List<OwmWeather> Weather { get; set; } = [];
        public OwmMain Main { get; set; } = new();
        public OwmWind? Wind { get; set; }
        public OwmSys? Sys { get; set; }
    }

    internal class OwmWeather
    {
        public string Main { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    internal class OwmMain
    {
        public double Temp { get; set; }
        public int Pressure { get; set; }
        public int Humidity { get; set; }
    }

    internal class OwmWind
    {
        public double Speed { get; set; }
        public int? Deg { get; set; }
    }

    internal class OwmSys
    {
        public string Country { get; set; } = string.Empty;
    }
}
