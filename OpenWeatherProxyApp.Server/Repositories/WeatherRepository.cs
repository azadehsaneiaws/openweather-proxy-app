using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using OpenWeatherProxyApp.Server.Models;
using Microsoft.Extensions.Logging;

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
        /// <returns>WeatherModel object containing weather details</returns>
        public async Task<WeatherModel> GetWeatherAsync(string city, string country)
        {
            // Get the current API key from the array
            string apiKey = ApiKeys[_currentApiKeyIndex];

            // Construct the request URL with city, country, and API key
            string requestUrl = $"{BaseUrl}?q={city},{country}&appid={apiKey}&units=metric";

            try
            {
                _logger.LogInformation($"Fetching weather for {city}, {country} using API key {apiKey}");

                // Make an asynchronous GET request to OpenWeather API
                var response = await _httpClient.GetAsync(requestUrl);

                // If the API returns a rate limit error (status 429), switch API keys
                if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    _logger.LogWarning("Rate limit exceeded. Switching API key...");

                    // Switch to the next API key
                    _currentApiKeyIndex = (_currentApiKeyIndex + 1) % ApiKeys.Length;
                    apiKey = ApiKeys[_currentApiKeyIndex];

                    // Retry the request with the new API key
                    requestUrl = $"{BaseUrl}?q={city},{country}&appid={apiKey}&units=metric";
                    response = await _httpClient.GetAsync(requestUrl);
                }

                // If the response is not successful, log an error and return null
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Failed to fetch weather for {city}, {country}. Status Code: {response.StatusCode}");
                    return null;
                }

                // Parse the response JSON into a dynamic object
                var data = await JsonSerializer.DeserializeAsync<JsonElement>(await response.Content.ReadAsStreamAsync());

                // Map JSON data to WeatherModel and return it
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
