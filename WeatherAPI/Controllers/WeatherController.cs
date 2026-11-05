using MediatR;
using Microsoft.AspNetCore.Mvc;
using WeatherAPI.App.Features;

namespace WeatherAPI.Controllers
{
    [ApiController]
    [Route("api/weather")]
    public class WeatherController(IMediator mediator) : ControllerBase
    {
        [HttpPost("notes")]
        public async Task<IActionResult> CreateNote(CreateWeatherNoteCommand command, CancellationToken cancellationToken)
        {
            var id = await mediator.Send(command, cancellationToken);
            return id is null ? NotFound() : StatusCode(StatusCodes.Status201Created, new { id });
        }

        [HttpGet("{cityName}")]
        public async Task<IActionResult> GetWeather(string cityName, [FromQuery] string? countryCode, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new GetWeatherQ { CityName = cityName, CountryCode = countryCode }, cancellationToken);
            return result is null ? NotFound() : Ok(result);
        }     
    }
}
