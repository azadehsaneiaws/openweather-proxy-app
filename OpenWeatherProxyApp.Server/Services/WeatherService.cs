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

        // Rate limit: 5 requests per API key per hour
        private static readonly ConcurrentDictionary<string, (int Count, DateTime Timestamp)> ApiKeyUsage = new();

        public WeatherService(IWeatherRepository weatherRepository, ILogger<WeatherService> logger, IConfiguration configuration)
        {
            _weatherRepository = weatherRepository;
            _logger = logger;
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<WeatherModel> GetWeatherAsync(string city, string country, string apiKey)
        {
            var allowedKeys = _configuration.GetSection("AllowedApiKeys").Get<string[]>() ?? Array.Empty<string>();

            if (!allowedKeys.Contains(apiKey))
            {
                _logger.LogWarning($"Unauthorized API key: {apiKey}");
                throw new UnauthorizedAccessException("Invalid API key.");
            }

            // Enforce rate limit (5 requests per hour)
            var currentTime = DateTime.UtcNow;
            ApiKeyUsage.AddOrUpdate(apiKey,
                _ => (1, currentTime),
                (_, usage) => usage.Timestamp.Hour == currentTime.Hour
                    ? (usage.Count + 1, usage.Timestamp)
                    : (1, currentTime) // Reset count when hour changes
            );

            if (ApiKeyUsage[apiKey].Count > 5)
            {
                _logger.LogWarning($"Rate limit exceeded for API key: {apiKey}");
                throw new InvalidOperationException("Rate limit exceeded. You can make up to 5 requests per hour.");
            }

            var openWeatherApiKeys = _configuration.GetSection("OpenWeatherApiKeys").Get<string[]>() ?? Array.Empty<string>();
            if (openWeatherApiKeys.Length == 0)
            {
                _logger.LogError("No OpenWeather API keys available.");
                throw new Exception("Missing OpenWeather API keys.");
            }

            var weatherData = await _weatherRepository.GetWeatherAsync(city, country, openWeatherApiKeys[0]);

            if (weatherData == null)
            {
                _logger.LogError($"Failed to fetch weather for {city}, {country}");
            }

            return weatherData;
        }
    }
}
