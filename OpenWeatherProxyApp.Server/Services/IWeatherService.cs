using OpenWeatherProxyApp.Server.Models;
using System.Threading.Tasks;

namespace OpenWeatherProxyApp.Server.Services
{
    /// <summary>
    /// Service interface for handling weather-related operations.
    /// </summary>
    public interface IWeatherService
    {
        /// <summary>
        /// Retrieves weather data for a given city and country.
        /// </summary>
        /// <param name="city">City name</param>
        /// <param name="country">Country code (e.g., "us" for the USA)</param>
        /// <returns>A WeatherModel object containing weather details.</returns>
        Task<WeatherModel> GetWeatherAsync(string city, string country);
    }
}
