using System;
using System.Collections.Generic;
using System.Text;
using WeatherAPI.App.Abstractions;
using WeatherAPI.Domain.Entities;

namespace WeatherAPI.Tests
{
    /// <summary>In-memory stand-in for ICountryRepository.</summary>
    public class FakeCountryRepository : ICountryRepository
    {
        public List<Country> Countries { get; } = [];

        public Task<IReadOnlyList<Country>> GetAllAsync(CancellationToken cancellationToken)
        {
            IReadOnlyList<Country> result = Countries.OrderBy(c => c.Name).ToList();
            return Task.FromResult(result);
        }

        public Task<bool> ExistsAsync(string countryCode, CancellationToken cancellationToken)
        {
            return Task.FromResult(Countries.Any(c => c.Code == countryCode));
        }
    }

    /// <summary>In-memory stand-in for ICityRepository.</summary>
    public class FakeCityRepository : ICityRepository
    {
        public List<City> Cities { get; } = [];

        public Task<IReadOnlyList<City>> GetByCountryCodeAsync(string countryCode, CancellationToken cancellationToken)
        {
            IReadOnlyList<City> result = Cities.Where(c => c.CountryCode == countryCode).OrderBy(c => c.Name).ToList();
            return Task.FromResult(result);
        }

        public Task<City?> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            return Task.FromResult(Cities.FirstOrDefault(c => c.Id == id));
        }

        public Task<City?> FindByNameAsync(string name, string? countryCode, CancellationToken cancellationToken)
        {
            var city = Cities.Where(c => string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase))
                .Where(c => countryCode == null || c.CountryCode == countryCode).OrderBy(c => c.Id).FirstOrDefault();
            return Task.FromResult(city);
        }
    }

    /// <summary>Returns a preset observation and records how it was called. No network access.</summary>
    public class FakeWeatherService : IWeatherService
    {
        public WeatherObservation? Result { get; set; }
        public int CallCount { get; private set; }
        public string? LastCityName { get; private set; }
        public string? LastCountryCode { get; private set; }

        public Task<WeatherObservation?> GetCurrentAsync(string cityName, string countryCode, CancellationToken cancellationToken)
        {
            CallCount++;
            LastCityName = cityName;
            LastCountryCode = countryCode;
            return Task.FromResult(Result);
        }
    }

    /// <summary>A clock that always returns the same moment, so timestamps are predictable in tests.</summary>
    public class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
