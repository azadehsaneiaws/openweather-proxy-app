using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenWeatherProxyApp.Server.Repositories;
using OpenWeatherProxyApp.Server.Services;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Register services for Dependency Injection (DI)
builder.Services.AddControllers();
builder.Services.AddHttpClient(); // Register HttpClient for API calls

// Register dependencies for Dependency Injection
builder.Services.AddScoped<IWeatherRepository, WeatherRepository>();
builder.Services.AddScoped<IWeatherService, WeatherService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "OpenWeather Proxy API",
        Version = "v1",
        Description = "A proxy service for fetching weather data from OpenWeather API."
    });
});
// Enabled CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
});


var app = builder.Build();
app.UseCors("AllowAll");

// Enable Swagger UI in development mode
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "OpenWeather Proxy API v1"));
}

app.UseRouting();
app.UseAuthorization();
app.MapControllers();
app.Run();
