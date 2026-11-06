using FluentValidation;
using MediatR;
using WeatherAPI.App.Abstractions;
using WeatherAPI.Domain.ValueObjects;

namespace WeatherAPI.App.Features
{
    /// <summary>
    /// Returns the current weather for a city, or null when the city is unknown.
    /// </summary>
    public class GetWeatherQ : IRequest<WeatherDto?>
    {
        public string CityName { get; set; } = string.Empty;
        public string? CountryCode { get; set; }
    }

    public class WeatherDto
    {
        public string City { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public DateTime ObservedAtUtc { get; set; }
        public double WindSpeedMph { get; set; }
        public int? WindDirectionDegrees { get; set; }
        public string? WindDirection { get; set; }
        public int? VisibilityMeters { get; set; }
        public int PressureHpa { get; set; }
        public string SkyCondition { get; set; } = string.Empty;
        public string SkyDescription { get; set; } = string.Empty;
        public double TemperatureFahrenheit { get; set; }
        public double TemperatureCelsius { get; set; }
        public double DewPointFahrenheit { get; set; }
        public double DewPointCelsius { get; set; }
        public int RelativeHumidityPercent { get; set; }
    }

    public class GetWeatherQueryValidator : AbstractValidator<GetWeatherQ>
    {
        public GetWeatherQueryValidator()
        {
            RuleFor(x => x.CityName).NotEmpty().MaximumLength(100);

            RuleFor(x => x.CountryCode).Matches("^[A-Za-z]{2}$").WithMessage("Country code must be a 2-letter ISO code.").When(x => !string.IsNullOrWhiteSpace(x.CountryCode));
        }
    }

    public class GetWeatherQueryHandler(ICityRepository cities, IWeatherService weatherService) : IRequestHandler<GetWeatherQ, WeatherDto?>
    {
        private static readonly string[] CompassPoints =
        [
            "N", "NNE", "NE", "ENE", "E", "ESE", "SE", "SSE","S", "SSW", "SW", "WSW", "W", "WNW", "NW", "NNW"
        ];

        public async Task<WeatherDto?> Handle(GetWeatherQ request, CancellationToken cancellationToken)
        {
            var countryCode = string.IsNullOrWhiteSpace(request.CountryCode) ? null : request.CountryCode.Trim().ToUpperInvariant();

            var city = await cities.FindByNameAsync(request.CityName.Trim(), countryCode, cancellationToken);
            if (city is null)
                return null;

            var observation = await weatherService.GetCurrentAsync(city.Name, city.CountryCode, cancellationToken);
            if (observation is null)
                return null;

            var dewPointF = Temperature.DewPointFahrenheit(observation.TemperatureFahrenheit, observation.HumidityPercent);

            return new WeatherDto
            {
                City = city.Name,
                Country = city.CountryCode,
                ObservedAtUtc = observation.ObservedAtUtc,
                WindSpeedMph = Math.Round(observation.WindSpeedMph, 1),
                WindDirectionDegrees = observation.WindDirectionDegrees,
                WindDirection = ToCompass(observation.WindDirectionDegrees),
                VisibilityMeters = observation.VisibilityMeters,
                PressureHpa = observation.PressureHpa,
                SkyCondition = observation.SkyCondition,
                SkyDescription = observation.SkyDescription,
                TemperatureFahrenheit = Math.Round(observation.TemperatureFahrenheit, 1),
                TemperatureCelsius = Math.Round(Temperature.FahrenheitToCelsius(observation.TemperatureFahrenheit), 1),
                DewPointFahrenheit = Math.Round(dewPointF, 1),
                DewPointCelsius = Math.Round(Temperature.FahrenheitToCelsius(dewPointF), 1),
                RelativeHumidityPercent = observation.HumidityPercent
            };
        }

        private static string? ToCompass(int? degrees)
        {
            if (degrees is null)
                return null;

            var index = (int)Math.Round(degrees.Value % 360 / 22.5) % 16;
            return CompassPoints[index];
        }
    }
}
