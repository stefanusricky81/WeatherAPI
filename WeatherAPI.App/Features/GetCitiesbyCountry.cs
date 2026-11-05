using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using WeatherAPI.App.Abstractions;

namespace WeatherAPI.App.Features
{
    public class GetCitiesByCountryQ : IRequest<IReadOnlyList<CityDto>?>
    {
        public string CountryCode { get; set; } = string.Empty;
    }

    public class CityDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string CountryCode { get; set; } = string.Empty;
    }

    public class GetCitiesByCountryQueryValidator : AbstractValidator<GetCitiesByCountryQ>
    {
        public GetCitiesByCountryQueryValidator()
        {
            RuleFor(x => x.CountryCode).NotEmpty().Matches("^[A-Za-z]{2}$").WithMessage("Country code must be 2 letter.");
        }
    }

    public class GetCitiesByCountryQueryHandler(ICountryRepository countries, ICityRepository cities) : IRequestHandler<GetCitiesByCountryQ, IReadOnlyList<CityDto>?>
    {
        public async Task<IReadOnlyList<CityDto>?> Handle(GetCitiesByCountryQ request, CancellationToken cancellationToken)
        {
            var countryCode = request.CountryCode.Trim().ToUpperInvariant();

            if (!await countries.ExistsAsync(countryCode, cancellationToken))
                return null;

            var result = await cities.GetByCountryCodeAsync(countryCode, cancellationToken);

            return result.Select(c => new CityDto { Id = c.Id, Name = c.Name, CountryCode = c.CountryCode }).ToList();
        }
    }
}
