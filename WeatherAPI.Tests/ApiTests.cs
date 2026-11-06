using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using WeatherAPI.App.Abstractions;
using WeatherAPI.App.Features;
using WeatherAPI.Infra;

namespace WeatherAPI.Tests
{
    public class WeatherApiFactory : WebApplicationFactory<Program>
    {
        private readonly string _dbName = Guid.NewGuid().ToString();
        public FakeWeatherService Weather { get; } = new()
        {
            Result = new WeatherObservation
            {
                CityName = "Jakarta",
                CountryCode = "ID",
                ObservedAtUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                TemperatureFahrenheit = 86,
                HumidityPercent = 70,
                PressureHpa = 1010,
                VisibilityMeters = 10000,
                WindSpeedMph = 5,
                WindDirectionDegrees = 90,
                SkyCondition = "Clear",
                SkyDescription = "clear sky"
            }
        };
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                // Replace SQLite with EF Core In-Memory.
                services.RemoveAll<DbContextOptions<AppDbContext>>();
                services.RemoveAll<IDbContextOptionsConfiguration<AppDbContext>>();
                services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(_dbName));

                // Replace the OpenWeatherMap client with a fake.
                services.RemoveAll<IWeatherService>();
                services.AddSingleton<IWeatherService>(Weather);
            });
        }
    }

    public class ApiTests(WeatherApiFactory factory) : IClassFixture<WeatherApiFactory>
    {
        private readonly HttpClient _client = factory.CreateClient();

        // ---------- Countries & cities ----------

        [Fact]
        public async Task GetCountries()
        {
            var countries = await _client.GetFromJsonAsync<List<CountryDto>>("/api/countries");

            Assert.NotNull(countries);
            Assert.Equal(7, countries.Count);
            Assert.Contains(countries, c => c.Code == "ID" && c.Name == "Indonesia");
        }

        [Fact]
        public async Task GetCities()
        {
            var cities = await _client.GetFromJsonAsync<List<CityDto>>("/api/countries/au/cities");

            Assert.NotNull(cities);
            Assert.Equal(4, cities.Count);
            Assert.All(cities, c => Assert.Equal("AU", c.CountryCode));
        }

        [Fact]
        public async Task GetInvalidCitiesCountry()
        {
            var response = await _client.GetAsync("/api/countries/XX/cities");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetCitiesInvalidCountry()
        {
            var response = await _client.GetAsync("/api/countries/123/cities");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            Assert.True(body.RootElement.GetProperty("errors").TryGetProperty("CountryCode", out _));
        }

        // ---------- Weather ----------

        [Fact]
        public async Task GetValidWeather()
        {
            var weather = await _client.GetFromJsonAsync<WeatherDto>("/api/weather/Jakarta?countryCode=ID");

            Assert.NotNull(weather);
            Assert.Equal("Jakarta", weather.City);
            Assert.Equal("ID", weather.Country);
            Assert.Equal(86, weather.TemperatureFahrenheit);
            Assert.Equal(30, weather.TemperatureCelsius);
            Assert.Equal("E", weather.WindDirection);
        }

        // ---------- Notes (command) ----------

        [Fact]
        public async Task CreateNoteValidData()
        {
            var response = await _client.PostAsJsonAsync("/api/weather/notes", new { cityId = 1, text = "Clear skies" });

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var id = body.RootElement.GetProperty("id").GetInt32();
            Assert.True(id > 0);

            using var scope = factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var note = await db.WeatherNotes.AsNoTracking().SingleAsync(n => n.Id == id);
            Assert.Equal("Clear skies", note.Text);
        }
    }
}
