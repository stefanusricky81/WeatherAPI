- CLAUDE.md

Instructions for AI assistants working on this repository.

-- Project

Weather API built with ASP.NET Core (.NET 10) in Visual Studio 2026.
Users pick a country and a city, see the current weather from OpenWeatherMap, and can save a note.

-- Architecture

The solution follows Onion Architecture. Each layer is a separate project:

- WeatherAPI.Domain - entities (Country, City, WeatherNote) and domain logic (Temperature). No dependencies.
- WeatherAPI.App - CQRS with MediatR: queries, commands, handlers, DTOs, validators and interfaces.
- WeatherAPI.Infra - EF Core (SQLite), repositories and the OpenWeatherMap client.
- WeatherAPI - controllers, error handling, Program.cs and the front-end (wwwroot).
- WeatherAPI.Tests - unit and integration tests.

-- Layer rules

- Dependencies only point inward: API -> Infra -> App -> Domain.
- Domain must not reference any package or project.
- App defines the interfaces, Infra implements them.
- Controllers only send requests through IMediator. No business logic in controllers.

-- CQRS

- One file per use case in WeatherAPI.App/Features, containing the request, DTO, validator and handler.
- Queries only read data. Commands change data.
- Validation runs in the MediatR pipeline (ValidationBehavior). Do not validate inside handlers.
- Keep MediatR on version 12.5.0 (the last free version).

-- Naming

- Queries end with Q: GetCountriesQ, GetWeatherQ.
- Commands end with Command: CreateWeatherNoteCommand.
- Handlers and validators use the full word: GetWeatherQueryHandler, CreateWeatherNoteCommandValidator.
- DTOs end with Dto: CountryDto, WeatherDto.
- Test methods use PascalCase without underscores: BelowAbsoluteZeroThrows.