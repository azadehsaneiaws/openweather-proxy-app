using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using OpenWeatherProxyApp.Server.Models;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.DataProtection.KeyManagement;

namespace OpenWeatherProxyApp.Server.Repositories
{
    /// <summary>
    /// Repository to fetch weather data from OpenWeather API.
    /// </summary>
    public class WeatherRepository : IWeatherRepository
    {
        private readonly HttpClient _httpClient; // HttpClient for making API requests
        private readonly ILogger<WeatherRepository> _logger; // Logger for debugging

        // Two API keys provided by OpenWeatherMap
        private static readonly string[] ApiKeys =
        {
            "8b7535b42fe1c551f18028f64e8688f7",
            "9f933451cebf1fa39de168a29a4d9a79"
        };

        private static int _currentApiKeyIndex = 0; // To track which API key to use

        private const string BaseUrl = "https://api.openweathermap.org/data/2.5/weather";

        /// <summary>
        /// Constructor with dependency injection for HttpClient and Logger.
        /// </summary>
        /// <param name="httpClient">HttpClient for making API requests</param>
        /// <param name="logger">Logger for debugging</param>
        public WeatherRepository(HttpClient httpClient, ILogger<WeatherRepository> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        /// <summary>
        /// Fetches weather data for a given city and country.
        /// </summary>
        /// <param name="city">City name</param>
        /// <param name="country">Country code (e.g., "us" for the USA)</param>
        /// <param name="apiKey">Optional API key</param>

        /// <returns>WeatherModel object containing weather details</returns>
        public async Task<WeatherModel> GetWeatherAsync(string city, string country, string? apiKey = null)
        {
            // Use provided API key if available, otherwise use the default one
            string selectedApiKey = string.IsNullOrWhiteSpace(apiKey) ? ApiKeys[_currentApiKeyIndex] : apiKey;

            string requestUrl = $"{BaseUrl}?q={city},{country}&appid={selectedApiKey}&units=metric";

            try
            {
                _logger.LogInformation($"Fetching weather for {city}, {country} using API key {selectedApiKey}");

                var response = await _httpClient.GetAsync(requestUrl);

                if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    _logger.LogWarning("Rate limit exceeded. Switching API key...");

                    _currentApiKeyIndex = (_currentApiKeyIndex + 1) % ApiKeys.Length;
                    selectedApiKey = ApiKeys[_currentApiKeyIndex];

                    requestUrl = $"{BaseUrl}?q={city},{country}&appid={selectedApiKey}&units=metric";
                    response = await _httpClient.GetAsync(requestUrl);
                }

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Failed to fetch weather for {city}, {country}. Status Code: {response.StatusCode}");
                    return null;
                }

                var data = await JsonSerializer.DeserializeAsync<JsonElement>(await response.Content.ReadAsStreamAsync());

                return new WeatherModel
                {
                    City = city,
                    Country = country,
                    Description = data.GetProperty("weather")[0].GetProperty("description").GetString(),
                    Temperature = data.GetProperty("main").GetProperty("temp").GetDouble(),
                    Humidity = data.GetProperty("main").GetProperty("humidity").GetDouble()
                };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"HTTP request failed: {ex.Message}");
                return null;
            }
            catch (JsonException ex)
            {
                _logger.LogError($"JSON parsing failed: {ex.Message}");
                return null;
            }
        }
    }
}