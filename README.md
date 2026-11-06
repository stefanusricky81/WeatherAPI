# Weather API

ASP.NET Core Web API (.NET 10) with a plain JavaScript front-end, built in Visual Studio 2026.
Users pick a country and a city, see the current weather from OpenWeatherMap, and can save a note.

## Features

- Country and city dropdowns loaded from the database.
- Current weather: location, time (UTC), wind, visibility, pressure, sky, temperature, dew point and humidity.
- Save a weather note. It is stored in SQLite together with the current weather.
- Unit and integration tests that run fully offline.

## Tech stack

- .NET 10, ASP.NET Core Web API
- MediatR 12.5.0 and FluentValidation
- EF Core 10 with SQLite (tests use EF Core In-Memory)
- xUnit
- Plain HTML, CSS and JavaScript

## Architecture

### Onion Architecture

The solution follows Onion Architecture. Each layer is a separate project:

- `WeatherAPI.Domain` - entities (`Country`, `City`, `WeatherNote`) and domain logic (`Temperature`). No dependencies.
- `WeatherAPI.App` - CQRS with MediatR: queries, commands, handlers, DTOs, validators and interfaces.
- `WeatherAPI.Infra` - EF Core (SQLite), repositories and the OpenWeatherMap client.
- `WeatherAPI` - controllers, error handling, `Program.cs` and the front-end.
- `WeatherAPI.Tests` - unit and integration tests.

Dependencies only point inward: API -> Infra -> App -> Domain.

App defines the interfaces and Infra implements them, so SQLite or OpenWeatherMap can be replaced without changing the business logic.
Each layer is a separate project, so the compiler enforces the dependency rules.

### CQRS with MediatR

- Queries only read data: `GetCountriesQ`, `GetCitiesByCountryQ`, `GetWeatherQ`.
- Commands change data: `CreateWeatherNoteCommand`.
- Controllers only send requests through `IMediator`. They contain no business logic.
- Validation runs once for every request in the MediatR pipeline (`ValidationBehavior`).

Request flow: Controller -> MediatR -> ValidationBehavior -> Handler -> Repository / Weather service.

## API endpoints

- `GET /api/countries` - all countries.
- `GET /api/countries/{countryCode}/cities` - cities of a country.
- `GET /api/weather/{cityName}?countryCode=AU` - current weather. `countryCode` is optional.
- `POST /api/weather/notes` - save a note. Body: `{ "cityId": 1, "text": "Sunny" }`. Returns `201` with the new id.

Errors use the ProblemDetails format: `400` validation error, `404` not found, `502` weather provider unavailable, `500` unexpected error.

## Getting started

### Requirements

- .NET 10 SDK
- Visual Studio 2026 (optional)
- OpenWeatherMap API key from https://openweathermap.org/api (new keys can take up to 2 hours to activate)

### Setup

1. Clone the repository:

   ```bash
   git clone https://github.com/stefanusricky81/WeatherAPI.git
   cd WeatherAPI
   ```

2. Set the API key with User Secrets:

   ```bash
   dotnet user-secrets set "OpenWeatherMap:ApiKey" "YOUR_API_KEY" --project WeatherAPI
   ```

   In Visual Studio you can also right click on the `WeatherAPI` project and choose **Manage User Secrets**.

### Build

```bash
dotnet build
```

### Run

```bash
dotnet run --project WeatherAPI --launch-profile https
```

Open https://localhost:7092/. In Visual Studio, set `WeatherAPI` as the startup project and press F5.

The SQLite database and seed data are created automatically on the first run.

### Test

```bash
dotnet test
```

Or in Visual Studio: **Test > Run All Tests**.

All tests run offline, so no API key is needed.

## Notes

### EF Core configuration

- The SQLite database is created on startup with `EnsureCreated()`, and countries and cities are seeded with `HasData`.
- All read queries use `.AsNoTracking()`.
- All dates are stored in UTC.

### State management

- Notes are saved in SQLite, so they are kept after a restart.
- EF Core only tracks changes when saving a note. Read queries do not track entities.
- The front-end keeps the loaded cities and the selected city in JavaScript variables.

### Design decisions

- Temperatures come from OpenWeatherMap in Fahrenheit (`units=imperial`) and are converted to Celsius in the Domain.
- Dew point is calculated from temperature and humidity, because OpenWeatherMap does not provide it.
- When a note is saved, the server gets the current weather itself. The client only sends the city and the text.
- MediatR 12.5.0 is used because newer versions need a paid licence.

### Tests

- `TemperatureTests` - Fahrenheit to Celsius edge cases (freezing, boiling, negative) and dew point.
- `QueryHandlerTests` - query handlers with fake repositories and a fake weather service.
- `CommandHandlerTests` - saving a note with EF Core In-Memory.
- `ApiTests` - the API running in memory with `WebApplicationFactory`.

### AI prompt file

`CLAUDE.md` in the solution root contains the instructions for AI coding assistants used during this project:
the architecture, layer rules, CQRS pattern and naming conventions.
Claude Code reads this file automatically.
