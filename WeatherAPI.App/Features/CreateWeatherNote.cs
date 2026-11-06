using FluentValidation;
using MediatR;
using WeatherAPI.App.Abstractions;
using WeatherAPI.Domain.Entities;

namespace WeatherAPI.App.Features
{
    public class CreateWeatherNoteCommand : IRequest<int?>
    {
        public int CityId { get; set; }
        public string Text { get; set; } = string.Empty;
    }

    public class CreateWeatherNoteCommandValidator : AbstractValidator<CreateWeatherNoteCommand>
    {
        public CreateWeatherNoteCommandValidator()
        {
            RuleFor(x => x.CityId).GreaterThan(0);

            RuleFor(x => x.Text).NotEmpty().MaximumLength(WeatherNote.MaxTextLength);
        }
    }

    public class CreateWeatherNoteCommandHandler(ICityRepository cities,IWeatherService weatherService,IWeatherNoteRepository notes,TimeProvider timeProvider) : IRequestHandler<CreateWeatherNoteCommand, int?>
    {
        public async Task<int?> Handle(CreateWeatherNoteCommand request, CancellationToken cancellationToken)
        {
            var city = await cities.GetByIdAsync(request.CityId, cancellationToken);
            if (city is null)
                return null;

            var observation = await weatherService.GetCurrentAsync(city.Name, city.CountryCode, cancellationToken);
            if (observation is null)
                return null;

            var note = new WeatherNote(
                city.Id,
                request.Text,
                observation.TemperatureFahrenheit,
                observation.SkyCondition,
                observation.ObservedAtUtc,
                timeProvider.GetUtcNow().UtcDateTime);

            await notes.AddAsync(note, cancellationToken);

            return note.Id;
        }
    }
}
