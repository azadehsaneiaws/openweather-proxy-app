﻿using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using OpenWeatherProxyApp.Server.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace OpenWeatherProxyApp.Server.Repositories
{
    public class WeatherRepository : IWeatherRepository
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<WeatherRepository> _logger;
        private readonly List<string> _apiKeys;
        private int _currentApiKeyIndex = 0;

        private const string BaseUrl = "https://api.openweathermap.org/data/2.5/weather";

        public WeatherRepository(HttpClient httpClient, ILogger<WeatherRepository> logger, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _logger = logger;
            _apiKeys = configuration.GetSection("OpenWeatherApiKeys").Get<List<string>>() ?? new List<string>();

            if (!_apiKeys.Any())
            {
                _logger.LogError("No OpenWeather API keys found in appsettings.json");
                throw new System.Exception("Missing OpenWeather API keys.");
            }
        }

        public async Task<WeatherModel> GetWeatherAsync(string city, string country, string? apiKey = null)
        {
            string selectedApiKey = string.IsNullOrWhiteSpace(apiKey) ? _apiKeys[_currentApiKeyIndex] : apiKey;
            string requestUrl = $"{BaseUrl}?q={city},{country}&appid={selectedApiKey}&units=metric";

            try
            {
                _logger.LogInformation($"Fetching weather for {city}, {country} using API key: {selectedApiKey}");

                var response = await _httpClient.GetAsync(requestUrl);
                _logger.LogInformation($"Received response: {response.StatusCode}");

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    _logger.LogError($"Unauthorized API key: {selectedApiKey}. Check OpenWeather API key.");
                    return null;
                }

                if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    _logger.LogWarning("OpenWeather API rate limit exceeded. Rotating API key...");

                    // Rotate API key
                    _currentApiKeyIndex = (_currentApiKeyIndex + 1) % _apiKeys.Count;
                    selectedApiKey = _apiKeys[_currentApiKeyIndex];

                    _logger.LogWarning($"Retrying with new API key: {selectedApiKey}");

                    requestUrl = $"{BaseUrl}?q={city},{country}&appid={selectedApiKey}&units=metric";
                    response = await _httpClient.GetAsync(requestUrl);
                }

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Failed to fetch weather for {city}, {country}. Status Code: {response.StatusCode}");
                    return null;
                }

                var responseContent = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrWhiteSpace(responseContent))
                {
                    _logger.LogError($"OpenWeather API returned empty response for {city}, {country}");
                    return null;
                }

                _logger.LogInformation($"Raw JSON Response: {responseContent}");

                JsonElement data;
                try
                {
                    data = JsonSerializer.Deserialize<JsonElement>(responseContent);
                }
                catch (JsonException ex)
                {
                    _logger.LogError($"JSON parsing failed: {ex.Message}");
                    return null;
                }

                if (!data.TryGetProperty("weather", out var weatherArray) || weatherArray.GetArrayLength() == 0 ||
                    !data.TryGetProperty("main", out var mainData))
                {
                    _logger.LogError("Unexpected API response structure. Missing required properties.");
                    return null;
                }

                var weather = new WeatherModel
                {
                    City = city,
                    Country = country,
                    Description = weatherArray[0].GetProperty("description").GetString(),
                    Temperature = mainData.GetProperty("temp").GetDouble(),
                    Humidity = mainData.GetProperty("humidity").GetDouble()
                };

                _logger.LogInformation($"Parsed Weather Data: {JsonSerializer.Serialize(weather)}");

                return weather;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"HTTP request failed: {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error fetching weather data: {ex.Message}");
                return null;
            }
        }
    }
}
