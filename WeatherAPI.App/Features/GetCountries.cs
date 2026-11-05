using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using WeatherAPI.App.Abstractions;

namespace WeatherAPI.App.Features
{
    public class GetCountriesQ : IRequest<IReadOnlyList<CountryDto>>
    {
    }

    public class CountryDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

    public class GetCountriesQueryHandler(ICountryRepository countries) : IRequestHandler<GetCountriesQ, IReadOnlyList<CountryDto>>
    {
        public async Task<IReadOnlyList<CountryDto>> Handle(GetCountriesQ request, CancellationToken cancellationToken)
        {
            var result = await countries.GetAllAsync(cancellationToken);

            return result.Select(c => new CountryDto { Code = c.Code, Name = c.Name }).ToList();
        }
    }
}
