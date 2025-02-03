import axios from "axios";

export const getWeather = async (city: string, country: string, apiKey: string) => {
  try {
    const response = await axios.get("https://localhost:7250/api/weather", {
      params: { city, country },
      headers: { "apiKey": apiKey }, 
    });
    return response.data;
  } catch (error) {
    console.error("Error fetching weather data:", error);
    throw new Error("Failed to fetch weather data.");
  }
};
