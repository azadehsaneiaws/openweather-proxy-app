using Microsoft.AspNetCore.Mvc;
using OpenWeatherProxyApp.Server.Models;
using OpenWeatherProxyApp.Server.Services;
using System;
using System.Linq;
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
        /// <returns>Weather details</returns>
        [HttpGet]
        public async Task<IActionResult> GetWeather(
            [FromQuery] string city,
            [FromQuery] string country,
            [FromServices] IHttpContextAccessor httpContextAccessor) // Inject HTTP Context
        {
            var headers = httpContextAccessor.HttpContext?.Request.Headers;

            if (headers == null)
            {
                return Unauthorized(new { error = "No headers found in the request." });
            }

            // Get API key, case insensitive
            string apiKey = headers.FirstOrDefault(h => string.Equals(h.Key, "apikey", StringComparison.OrdinalIgnoreCase)).Value;

            Console.WriteLine($"Received API Key: {apiKey}");

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                return Unauthorized(new { error = "API key is required." });
            }

            try
            {
                var weather = await _weatherService.GetWeatherAsync(city, country, apiKey);

                if (weather == null)
                {
                    return NotFound(new { error = "Weather data not found. Please check the city and country." });
                }

                return Ok(weather);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { error = "Invalid API key." });
            }
            catch (InvalidOperationException)
            {
                return StatusCode(429, new { error = "Rate limit exceeded. You can make up to 5 requests per hour." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: {ex.Message}"); // Log actual error
                return StatusCode(500, new { error = "An internal server error occurred. Check backend logs." });
            }
        }
    }
}
