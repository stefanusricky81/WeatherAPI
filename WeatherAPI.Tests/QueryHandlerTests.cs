using System;
using System.Collections.Generic;
using System.Text;
using WeatherAPI.App.Abstractions;
using WeatherAPI.App.Features;
using WeatherAPI.Domain.Entities;

namespace WeatherAPI.Tests
{
    public class QueryHandlerTests
    {
        [Fact]
        public async Task ReturnsAllCountriesSortedByName()
        {
            var countries = new FakeCountryRepository();
            countries.Countries.Add(new Country("SG", "Singapore"));
            countries.Countries.Add(new Country("AU", "Australia"));

            var handler = new GetCountriesQueryHandler(countries);

            var result = await handler.Handle(new GetCountriesQ(), CancellationToken.None);

            Assert.Equal(2, result.Count);
            Assert.Equal("AU", result[0].Code);
            Assert.Equal("Australia", result[0].Name);
            Assert.Equal("SG", result[1].Code);
        }

        [Fact]
        public async Task GetCitiesByCountry_ReturnsNull()
        {
            var handler = new GetCitiesByCountryQueryHandler(new FakeCountryRepository(), new FakeCityRepository());

            var result = await handler.Handle(new GetCitiesByCountryQ { CountryCode = "XX" }, CancellationToken.None);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetCitiesByCountry_ReturnsOnlyCitiesCountryAndAllowLowerCase()
        {
            var countries = new FakeCountryRepository();
            countries.Countries.Add(new Country("AU", "Australia"));
            countries.Countries.Add(new Country("GB", "United Kingdom"));

            var cities = new FakeCityRepository();
            cities.Cities.Add(new City(1, "Sydney", "AU"));
            cities.Cities.Add(new City(2, "Perth", "AU"));
            cities.Cities.Add(new City(3, "London", "GB"));

            var handler = new GetCitiesByCountryQueryHandler(countries, cities);

            var result = await handler.Handle(new GetCitiesByCountryQ { CountryCode = "au" }, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(new[] { "Perth", "Sydney" }, result.Select(c => c.Name));
            Assert.All(result, c => Assert.Equal("AU", c.CountryCode));
        }

        [Fact]
        public async Task GetWeather_CityNotInDBReturnsNullAndNotCallProvider()
        {
            var weather = new FakeWeatherService { Result = SampleObservation() };
            var handler = new GetWeatherQueryHandler(new FakeCityRepository(), weather);

            var result = await handler.Handle(new GetWeatherQ { CityName = "Atlantis" }, CancellationToken.None);

            Assert.Null(result);
            Assert.Equal(0, weather.CallCount);
        }

        [Fact]
        public async Task GetWeather_UnknownKnowCityReturnsNull()
        {
            var cities = new FakeCityRepository();
            cities.Cities.Add(new City(1, "Jakarta", "ID"));

            var weather = new FakeWeatherService { Result = null };
            var handler = new GetWeatherQueryHandler(cities, weather);

            var result = await handler.Handle(new GetWeatherQ { CityName = "Jakarta", CountryCode = "ID" }, CancellationToken.None);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetWeather_UseCountryCodeToCity()
        {
            var cities = new FakeCityRepository();
            cities.Cities.Add(new City(8, "Perth", "AU"));
            cities.Cities.Add(new City(15, "Perth", "GB"));

            var weather = new FakeWeatherService { Result = SampleObservation() };
            var handler = new GetWeatherQueryHandler(cities, weather);

            await handler.Handle(new GetWeatherQ { CityName = "perth", CountryCode = "gb" }, CancellationToken.None);

            Assert.Equal("Perth", weather.LastCityName);
            Assert.Equal("GB", weather.LastCountryCode);
        }

        [Fact]
        public async Task GetWeather_MapsPayloadConvertsFahrenheitToCelsius()
        {
            var cities = new FakeCityRepository();
            cities.Cities.Add(new City(1, "Jakarta", "ID"));

            var observedAt = new DateTime(2026, 1, 1, 6, 0, 0, DateTimeKind.Utc);
            var weather = new FakeWeatherService { Result = SampleObservation(observedAt) };
            var handler = new GetWeatherQueryHandler(cities, weather);

            var result = await handler.Handle(new GetWeatherQ { CityName = "Jakarta", CountryCode = "ID" }, CancellationToken.None);

            Assert.NotNull(result);

            // Location & time
            Assert.Equal("Jakarta", result.City);
            Assert.Equal("ID", result.Country);
            Assert.Equal(observedAt, result.ObservedAtUtc);

            // Temperature: 86 °F = 30 °C
            Assert.Equal(86, result.TemperatureFahrenheit);
            Assert.Equal(30, result.TemperatureCelsius);

            // Dew point at 30 °C / 70 % RH ≈ 23.9 °C ≈ 75.1 °F (Magnus)
            Assert.Equal(23.9, result.DewPointCelsius);
            Assert.Equal(75.1, result.DewPointFahrenheit);
            Assert.Equal(70, result.RelativeHumidityPercent);

            // Wind: rounded to 1 decimal, 225° = SW
            Assert.Equal(12.3, result.WindSpeedMph);
            Assert.Equal(225, result.WindDirectionDegrees);
            Assert.Equal("SW", result.WindDirection);

            // Visibility, pressure, sky
            Assert.Equal(10000, result.VisibilityMeters);
            Assert.Equal(1010, result.PressureHpa);
            Assert.Equal("Clouds", result.SkyCondition);
            Assert.Equal("scattered clouds", result.SkyDescription);
        }

        [Fact]
        public async Task GetWeather_NoWindDirectionReturnsNullCompass()
        {
            var cities = new FakeCityRepository();
            cities.Cities.Add(new City(1, "Jakarta", "ID"));

            var observation = SampleObservation();
            observation.WindDirectionDegrees = null;

            var handler = new GetWeatherQueryHandler(cities, new FakeWeatherService { Result = observation });

            var result = await handler.Handle(new GetWeatherQ { CityName = "Jakarta" }, CancellationToken.None);

            Assert.Null(result!.WindDirection);
        }

        [Theory]
        [InlineData(225, "SW")]
        [InlineData(355, "N")]
        public async Task GetWeather_ConvertsWindDegreesToCompass(int degrees, string expected)
        {
            var cities = new FakeCityRepository();
            cities.Cities.Add(new City(1, "Jakarta", "ID"));

            var observation = SampleObservation();
            observation.WindDirectionDegrees = degrees;

            var handler = new GetWeatherQueryHandler(cities, new FakeWeatherService { Result = observation });

            var result = await handler.Handle(new GetWeatherQ { CityName = "Jakarta" }, CancellationToken.None);

            Assert.Equal(expected, result!.WindDirection);
        }
        // ---------- helpers ----------

        private static WeatherObservation SampleObservation(DateTime? observedAtUtc = null) => new()
        {
            CityName = "Jakarta",
            CountryCode = "ID",
            ObservedAtUtc = observedAtUtc ?? new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            TemperatureFahrenheit = 86,
            HumidityPercent = 70,
            PressureHpa = 1010,
            VisibilityMeters = 10000,
            WindSpeedMph = 12.34,
            WindDirectionDegrees = 225,
            SkyCondition = "Clouds",
            SkyDescription = "scattered clouds"
        };

    }
}
