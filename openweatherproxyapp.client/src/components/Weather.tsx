import React, { useState } from "react";
import { getWeather } from "../services/weatherService";
import { WeatherModel } from "../models/WeatherModel";
import { Card, Button, Form, Alert, Row, Col, Container } from "react-bootstrap";

const Weather: React.FC = () => {
  const [city, setCity] = useState<string>("");
  const [country, setCountry] = useState<string>("");
  const [apiKey, setApiKey] = useState<string>("");
  const [weather, setWeather] = useState<WeatherModel | null>(null);
  const [error, setError] = useState<string>("");

  const fetchWeather = async () => {
    if (!city || !country || !apiKey) {
      setError("City, Country, and API Key are required!");
      return;
    }
  
    try {
      setError("");
      console.log("Sending API Key:", apiKey);
      const data = await getWeather(city, country, apiKey);
      setWeather(data);
    } catch (err: unknown) {
      setWeather(null);
      setError(err instanceof Error ? err.message : "An unknown error occurred.");
    }
  };
  

  return (
    <Container fluid className="d-flex justify-content-center align-items-center vh-100">
      <Row className="justify-content-right w-100">
        <Col xs={12} md={12} lg={12}>
          <Card className="shadow-lg p-4">
            <Card.Header className="bg-primary text-white text-center">
              <h4 className="mb-0">Weather Lookup</h4>
            </Card.Header>
            <Card.Body>
              <Form>
                <Form.Group className="mb-3" controlId="formCity">
                  <Form.Label>City</Form.Label>
                  <Form.Control
                    type="text"
                    placeholder="Enter city"
                    value={city}
                    onChange={(e) => setCity(e.target.value)}
                  />
                </Form.Group>

                <Form.Group className="mb-3" controlId="formCountry">
                  <Form.Label>Country</Form.Label>
                  <Form.Control
                    type="text"
                    placeholder="Enter country"
                    value={country}
                    onChange={(e) => setCountry(e.target.value)}
                  />
                </Form.Group>

                <Form.Group className="mb-3" controlId="formApiKey">
                  <Form.Label>API Key</Form.Label>
                  <Form.Control
                    type="text"
                    placeholder="Enter API Key"
                    value={apiKey}
                    onChange={(e) => setApiKey(e.target.value)}
                  />
                </Form.Group>

                <Button variant="primary" className="w-100" onClick={fetchWeather}>
                  Get Weather
                </Button>
              </Form>

              {error && <Alert variant="danger" className="mt-3 text-center">{error}</Alert>}

              {weather && (
                <Alert variant="info" className="mt-3 text-center">
                  <h5>{weather.description}</h5>
                  <p>Temperature: {weather.temperature}°C</p>
                  <p>Humidity: {weather.humidity}%</p>
                </Alert>
              )}
            </Card.Body>
          </Card>
        </Col>
      </Row>
    </Container>
  );
};

export default Weather;
