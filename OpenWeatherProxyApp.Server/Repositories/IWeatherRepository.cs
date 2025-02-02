using OpenWeatherProxyApp.Server.Models;

namespace OpenWeatherProxyApp.Server.Repositories
{
    public interface IWeatherRepository
    {
        Task<WeatherModel> GetWeatherAsync(string city, string country);

    }
}
