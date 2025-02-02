namespace OpenWeatherProxyApp.Server.Models
{
    public class WeatherModel
    {
        public string City { get; set; }
        public string Country { get; set; }
        public string Description { get; set; }
        public double Temperature { get; set; }
        public double Humidity { get; set; }
    }
}
