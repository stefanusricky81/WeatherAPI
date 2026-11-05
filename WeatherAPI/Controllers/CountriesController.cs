using MediatR;
using Microsoft.AspNetCore.Mvc;
using WeatherAPI.App.Features;

namespace WeatherAPI.Controllers
{
    [ApiController]
    [Route("api/countries")]
    public class CountriesController(IMediator mediator) : ControllerBase
    {
        [HttpGet("{countryCode}/cities")]
        public async Task<IActionResult> GetCities(string countryCode, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new GetCitiesByCountryQ { CountryCode = countryCode }, cancellationToken);
            return result is null ? NotFound() : Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetCountries(CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new GetCountriesQ(), cancellationToken);
            return Ok(result);
        }
    }
}
