using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using WeatherAPI.App.Abstractions;
using WeatherAPI.App.Features;
using WeatherAPI.Infra;

namespace WeatherAPI.Tests
{
    public class CommandHandlerTests
    {
        private static readonly DateTimeOffset Now = new(2026, 1, 1, 8, 0, 0, TimeSpan.Zero);
        private static readonly DateTime ObservedAt = new(2026, 1, 1, 7, 30, 0, DateTimeKind.Utc);

        [Fact]
        public async Task CreateNote_ValidCommandAndPersistNoteWithWeatherSnapshot()
        {
            var dbName = Guid.NewGuid().ToString();
            using var db = CreateDb(dbName);
            var handler = CreateHandler(db, Observation(), out _);

            var id = await handler.Handle(
                new CreateWeatherNoteCommand { CityId = 1, Text = "  Sunny morning  " },
                CancellationToken.None);

            Assert.NotNull(id);
            Assert.True(id > 0);

            // Read back through a NEW context to prove the row was persisted, not just tracked.
            using var verifyDb = CreateDb(dbName);
            var note = await verifyDb.WeatherNotes.AsNoTracking().SingleAsync();

            Assert.Equal(id, note.Id);
            Assert.Equal(1, note.CityId);
            Assert.Equal("Sunny morning", note.Text);               // trimmed by the entity
            Assert.Equal(86, note.Fahrenheit);                      // snapshot from the provider
            Assert.Equal("Clear", note.SkyCondition);
            Assert.Equal(ObservedAt, note.ObservedAtUtc);
            Assert.Equal(Now.UtcDateTime, note.CreatedAtUtc);       // from the injected clock
        }

        [Fact]
        public async Task CreateNote_TwoCommandsGenerateDistinctIds()
        {
            var dbName = Guid.NewGuid().ToString();
            using var db = CreateDb(dbName);
            var handler = CreateHandler(db, Observation(), out _);

            var first = await handler.Handle(new CreateWeatherNoteCommand { CityId = 1, Text = "First" }, CancellationToken.None);
            var second = await handler.Handle(new CreateWeatherNoteCommand { CityId = 1, Text = "Second" }, CancellationToken.None);

            Assert.NotEqual(first, second);

            using var verifyDb = CreateDb(dbName);
            Assert.Equal(2, await verifyDb.WeatherNotes.CountAsync());
        }

        [Fact]
        public async Task CreateNote_UnknownCityReturnsNullPersistsNothing()
        {
            var dbName = Guid.NewGuid().ToString();
            using var db = CreateDb(dbName);
            var handler = CreateHandler(db, Observation(), out var weather);

            var id = await handler.Handle(new CreateWeatherNoteCommand { CityId = 999, Text = "Hello" }, CancellationToken.None);

            Assert.Null(id);
            Assert.Equal(0, weather.CallCount);

            using var verifyDb = CreateDb(dbName);
            Assert.Equal(0, await verifyDb.WeatherNotes.CountAsync());
        }

        [Fact]
        public async Task CreateNote_ProviderNotKnowCityReturnsNullAndPersistsNothing()
        {
            var dbName = Guid.NewGuid().ToString();
            using var db = CreateDb(dbName);
            var handler = CreateHandler(db, observation: null, out _);

            var id = await handler.Handle(new CreateWeatherNoteCommand { CityId = 1, Text = "Hello" }, CancellationToken.None);

            Assert.Null(id);

            using var verifyDb = CreateDb(dbName);
            Assert.Equal(0, await verifyDb.WeatherNotes.CountAsync());
        }

        // ---------- helpers ----------

        private static AppDbContext CreateDb(string dbName)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;

            var db = new AppDbContext(options);
            db.Database.EnsureCreated();   // applies seed data (countries & cities)
            return db;
        }

        private static CreateWeatherNoteCommandHandler CreateHandler(
            AppDbContext db,
            WeatherObservation? observation,
            out FakeWeatherService weather)
        {
            weather = new FakeWeatherService { Result = observation };

            return new CreateWeatherNoteCommandHandler(
                new CityRepository(db),
                weather,
                new WeatherNoteRepository(db),
                new FixedTimeProvider(Now));
        }

        private static WeatherObservation Observation() => new()
        {
            CityName = "Jakarta",
            CountryCode = "ID",
            ObservedAtUtc = ObservedAt,
            TemperatureFahrenheit = 86,
            HumidityPercent = 70,
            PressureHpa = 1010,
            VisibilityMeters = 10000,
            WindSpeedMph = 5,
            WindDirectionDegrees = 90,
            SkyCondition = "Clear",
            SkyDescription = "clear sky"
        };
    }
}