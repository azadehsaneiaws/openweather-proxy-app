using OpenWeatherProxyApp.Server.Models;
using OpenWeatherProxyApp.Server.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Collections.Concurrent;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OpenWeatherProxyApp.Server.Services
{
    public class WeatherService : IWeatherService
    {
        private readonly IWeatherRepository _weatherRepository;
        private readonly ILogger<WeatherService> _logger;
        private readonly IConfiguration _configuration;

        // Track API key usage (request count per API key per hour)
        private static readonly ConcurrentDictionary<string, (int count, DateTime timestamp)> ApiKeyUsage = new();

        public WeatherService(IWeatherRepository weatherRepository, ILogger<WeatherService> logger, IConfiguration configuration)
        {
            _weatherRepository = weatherRepository;
            _logger = logger;
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<WeatherModel> GetWeatherAsync(string city, string country, string apiKey)
        {
            var allowedKeysString = _configuration.GetSection("AllowedApiKeys").Value;
            var allowedKeys = allowedKeysString?.Split(',') ?? Array.Empty<string>();

            if (!allowedKeys.Contains(apiKey))
            {
                _logger.LogWarning($"Unauthorized API key: {apiKey}");
                throw new UnauthorizedAccessException("Invalid API key.");
            }

            // Enforce rate limit (5 requests per hour)
            if (ApiKeyUsage.TryGetValue(apiKey, out var usage))
            {
                if (usage.timestamp.Hour == DateTime.UtcNow.Hour && usage.count >= 5)
                {
                    _logger.LogWarning($"Rate limit exceeded for API key: {apiKey}");
                    throw new InvalidOperationException("Rate limit exceeded. You can make up to 5 requests per hour.");
                }
            }

            // Fetch weather data
            var weatherData = await _weatherRepository.GetWeatherAsync(city, country, apiKey);

            if (weatherData == null)
            {
                _logger.LogError($"Failed to fetch weather for {city}, {country}");
            }

            return weatherData;
        }

    }
}
