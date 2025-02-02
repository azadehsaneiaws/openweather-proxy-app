export const getWeather = async (city: string, country: string, apiKey: string) => {
    const response = await fetch(
      `http://localhost:7250/api/weather?city=${city}&country=${country}`,
      {
        method: "GET",
        headers: {
          "Content-Type": "application/json",
          "apiKey": apiKey, //  API Key should be sent in the header
        },
      }
    );
  
    if (!response.ok) {
      throw new Error("Failed to fetch weather data. Check API key or input.");
    }
  
    return response.json();
  };
  