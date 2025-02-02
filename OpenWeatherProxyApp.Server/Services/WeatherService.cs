using OpenWeatherProxyApp.Server.Models;
using OpenWeatherProxyApp.Server.Repositories;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace OpenWeatherProxyApp.Server.Services
{
    /// <summary>
    /// Weather service that handles business logic for retrieving weather data.
    /// </summary>
    public class WeatherService : IWeatherService
    {
        private readonly IWeatherRepository _weatherRepository;
        private readonly ILogger<WeatherService> _logger;

        /// <summary>
        /// Constructor for WeatherService with dependency injection.
        /// </summary>
        /// <param name="weatherRepository">Injected repository for weather data retrieval</param>
        /// <param name="logger">Injected logger for debugging</param>
        public WeatherService(IWeatherRepository weatherRepository, ILogger<WeatherService> logger)
        {
            _weatherRepository = weatherRepository;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves weather data for a given city and country.
        /// Applies additional business logic if needed.
        /// </summary>
        /// <param name="city">City name</param>
        /// <param name="country">Country code (e.g., "us" for the USA)</param>
        /// <param name="apiKey">Optional API key to use for the request</param>

        /// <returns>WeatherModel containing weather details</returns>
        public async Task<WeatherModel> GetWeatherAsync(string city, string country, string? apiKey = null)
        {
            _logger.LogInformation($"Fetching weather data for {city}, {country} using API key {apiKey ?? "Default"}");

            // Validate input parameters
            if (string.IsNullOrWhiteSpace(city) || string.IsNullOrWhiteSpace(country))
            {
                _logger.LogWarning("Invalid city or country provided.");
                return null;
            }

            // Call repository with optional API key
            var weather = await _weatherRepository.GetWeatherAsync(city, country, apiKey);

            if (weather == null)
            {
                _logger.LogError($"Failed to fetch weather for {city}, {country}");
            }

            return weather;
        }
    }
}