import React, { useState } from "react";
import { getWeather } from "../services/weatherService";
import { WeatherModel } from "../models/WeatherModel";
import "./Weather.css"; 

const Weather: React.FC = () => {
  const [city, setCity] = useState<string>("");
  const [country, setCountry] = useState<string>("");
  const [apiKey, setApiKey] = useState<string>("");
  const [weather, setWeather] = useState<WeatherModel | null>(null);
  const [error, setError] = useState<string>("");
  const [loading, setLoading] = useState<boolean>(false);

  const fetchWeather = async () => {
    if (!city || !country || !apiKey) {
      setError("City, Country, and API Key are required!");
      return;
    }

    try {
      setError("");
      setLoading(true);
      const data = await getWeather(city, country, apiKey);
      setWeather(data);
    } catch (err: unknown) {
      setWeather(null);
      setError(err instanceof Error ? err.message : "An unknown error occurred.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="weather-container">
      <h2>Weather Lookup</h2>
      
      <input type="text" placeholder="City" value={city} onChange={(e) => setCity(e.target.value)} />
      <input type="text" placeholder="Country" value={country} onChange={(e) => setCountry(e.target.value)} />
      <input type="text" placeholder="API Key" value={apiKey} onChange={(e) => setApiKey(e.target.value)} />

      <button onClick={fetchWeather} disabled={loading}>
        {loading ? "Fetching..." : "Get Weather"}
      </button>

      {error && <p className="error">{error}</p>}

      {weather && (
        <div className="weather-info">
          <h3>{weather.description}</h3>
          <p>Temperature: {weather.temperature}°C</p>
          <p>Humidity: {weather.humidity}%</p>
        </div>
      )}
    </div>
  );
};

export default Weather;
