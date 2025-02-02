using Microsoft.AspNetCore.Mvc;
using OpenWeatherProxyApp.Server.Models;
using OpenWeatherProxyApp.Server.Services;
using System.Threading.Tasks;

namespace OpenWeatherProxyApp.Server.Controllers
{
    /// <summary>
    /// API Controller for fetching weather data.
    /// </summary>
    [Route("api/weather")]
    [ApiController]
    public class WeatherController : ControllerBase
    {
        private readonly IWeatherService _weatherService;

        /// <summary>
        /// Constructor for WeatherController.
        /// </summary>
        /// <param name="weatherService">Injected weather service</param>
        public WeatherController(IWeatherService weatherService)
        {
            _weatherService = weatherService;
        }

        /// <summary>
        /// Retrieves weather data for a given city and country.
        /// Optionally allows specifying an API key.
        /// </summary>
        /// <param name="city">City name</param>
        /// <param name="country">Country code</param>
        /// <param name="apiKey">Optional API key to use for the request</param>
        /// <returns>Weather details</returns>
        [HttpGet]
        public async Task<IActionResult> GetWeather(
            [FromQuery] string city,
            [FromQuery] string country,
            [FromQuery] string? apiKey = null)
        {
            if (string.IsNullOrWhiteSpace(city) || string.IsNullOrWhiteSpace(country))
            {
                return BadRequest("City and country are required.");
            }

            var weather = await _weatherService.GetWeatherAsync(city, country, apiKey);

            if (weather == null)
            {
                return NotFound("Weather data not found.");
            }

            return Ok(weather);
        }
    }
}