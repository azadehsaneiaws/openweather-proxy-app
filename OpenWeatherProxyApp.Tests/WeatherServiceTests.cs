using System.Threading.Tasks;
using Xunit;
using Moq;
using OpenWeatherProxyApp.Server.Repositories;
using OpenWeatherProxyApp.Server.Services;
using Microsoft.Extensions.Logging;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using OpenWeatherProxyApp.Server.Models;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Collections.Concurrent;

namespace OpenWeatherProxyApp.Tests.Services
{
    public class WeatherServiceTests
    {
        private readonly WeatherService _weatherService;
        private readonly Mock<IWeatherRepository> _weatherRepositoryMock;
        private readonly Mock<ILogger<WeatherService>> _loggerMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<IConfigurationSection> _allowedApiKeysSectionMock;

        public WeatherServiceTests()
        {
            _weatherRepositoryMock = new Mock<IWeatherRepository>();
            _loggerMock = new Mock<ILogger<WeatherService>>();
            _configurationMock = new Mock<IConfiguration>();
            _allowedApiKeysSectionMock = new Mock<IConfigurationSection>();

            // FIX: Properly return the API keys as a comma-separated string
            var allowedApiKeysString = "APIKEY-12345,APIKEY-67890,APIKEY-77177";

            _allowedApiKeysSectionMock
                .Setup(x => x.Value)
                .Returns(allowedApiKeysString);

            _configurationMock
                .Setup(c => c.GetSection("AllowedApiKeys"))
                .Returns(_allowedApiKeysSectionMock.Object);

            // Initialize the WeatherService with all dependencies
            _weatherService = new WeatherService(
                _weatherRepositoryMock.Object,
                _loggerMock.Object,
                _configurationMock.Object
            );
        }

        [Fact]
        public async Task GetWeatherAsync_ShouldReturnWeatherData_WhenValidInput()
        {
            var city = "California";
            var country = "us";
            var apiKey = "APIKEY-12345";
            var expectedWeather = new WeatherModel { Description = "Sunny" };

            _weatherRepositoryMock
                .Setup(repo => repo.GetWeatherAsync(city, country, apiKey))
                .ReturnsAsync(expectedWeather);

            var result = await _weatherService.GetWeatherAsync(city, country, apiKey);

            result.Should().NotBeNull();
            result.Description.Should().Be("Sunny");
        }

        [Fact]
        public async Task GetWeatherAsync_ShouldReturnNull_WhenApiFails()
        {
            var city = "InvalidCity";
            var country = "InvalidCountry";
            var apiKey = "APIKEY-67890";

            _weatherRepositoryMock
                .Setup(repo => repo.GetWeatherAsync(city, country, apiKey))
                .ReturnsAsync((WeatherModel)null);

            var result = await _weatherService.GetWeatherAsync(city, country, apiKey);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetWeatherAsync_ShouldLogError_WhenWeatherDataIsNull()
        {
            // Arrange
            var city = "NonExistentCity";
            var country = "us";
            var apiKey = "APIKEY-77177";

            _weatherRepositoryMock
                .Setup(repo => repo.GetWeatherAsync(city, country, apiKey))
                .ReturnsAsync((WeatherModel)null);

            // Act
            var result = await _weatherService.GetWeatherAsync(city, country, apiKey);

            // Assert
            _loggerMock.Verify(
                log => log.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Failed to fetch weather for {city}, {country}")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()
                ), Times.Once);

            result.Should().BeNull();
        }


        [Fact]
        public async Task GetWeatherAsync_ShouldThrowUnauthorizedAccessException_WhenApiKeyIsInvalid()
        {
            var city = "California";
            var country = "us";
            var invalidApiKey = "INVALID-API-KEY";

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _weatherService.GetWeatherAsync(city, country, invalidApiKey));
        }
        [Fact]
        public async Task GetWeatherAsync_ShouldThrowInvalidOperationException_WhenRateLimitExceeded()
        {
            var city = "California";
            var country = "us";
            var apiKey = "APIKEY-12345";

            // Retrieve the rate limit dictionary correctly
            var apiKeyUsageField = typeof(WeatherService)
                .GetField("ApiKeyUsage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

            var apiKeyUsage = apiKeyUsageField?.GetValue(null) as ConcurrentDictionary<string, (int count, DateTime timestamp)>;

            //  Ensure the dictionary is cleared before running the test
            apiKeyUsage?.Clear();

            // Manually insert API key usage to simulate 5 previous calls within the same hour
            apiKeyUsage?.TryAdd(apiKey, (5, DateTime.UtcNow));

            // Act & Assert - The next (6th) request should fail due to rate limit
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _weatherService.GetWeatherAsync(city, country, apiKey));

            exception.Message.Should().Be("Rate limit exceeded. You can make up to 5 requests per hour.");
        }


    }
}
