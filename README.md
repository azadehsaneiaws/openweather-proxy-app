
# OpenWeather Proxy App 

This is a weather lookup application built with **React (Vite)** for the frontend and **.NET 8 Web API** for the backend. It acts as a proxy to fetch weather data using OpenWeather API.

##  Features
- Frontend: React (Vite) + TypeScript
- Backend: .NET 8 Web API with rate-limited access
- Secure API key handling
- Proper error handling and logging
- External API integration with OpenWeather API

##  Setup Instructions

### **1️⃣ Clone the repository**
git clone https://github.com/azadehsaneiaws/openweatherproxyapp.git
cd openweatherproxyapp

### **2️⃣  Setup Backend**
cd OpenWeatherProxyApp.Server
dotnet restore
dotnet build

I added Swagger so you Can Test the Backend And Rate Limit Logic, Here: https://localhost:7250/swagger/index.html


### **3️⃣  Setup Frontend**
cd ../openweatherproxyapp.client
npm install

### **4️⃣ Run Backend and Frontend Together**
Instead of manually running them in two terminals, use the PowerShell script below.

### Run Project Automatically (PowerShell)

I added a powershell script to the project root, Open Powershell in project root and execute 
./start-app.ps1
Now, open http://localhost:5173 in your browser!

## Common Issues & Fixes

### Port already in use

If you see "address already in use", stop the previous instance:

Get-Process | Where-Object { $_.ProcessName -like "dotnet" } | Stop-Process
Get-Process | Where-Object { $_.ProcessName -like "node" } | Stop-Process

### API calls fail (CORS issues)

Ensure that backend launchSettings.json allows requests from the frontend running on http://localhost:5173.

### Backend not reachable

If https://localhost:7250/swagger/index.html doesn't open, make sure that:

The backend is running (dotnet run in OpenWeatherProxyApp.Server).

vite.config.ts is correctly proxying API requests.
## Project Design Pattern & Technical Overview
This project follows a Clean Architecture approach with Separation of Concerns (SoC), ensuring modularity, scalability, and maintainability.

### Backend (ASP.NET Core - OpenWeather Proxy API)
Pattern Used: Repository Pattern + Dependency Injection (DI)
Architecture:
Controllers (Presentation Layer): Handles HTTP requests and delegates to services.
Services (Business Logic Layer): Implements the core logic, interacts with repositories, and enforces rate-limiting.
Repositories (Data Layer): Abstracts API calls to OpenWeather, ensuring maintainability and testability.
Middleware: Custom middleware for API Key validation and Rate Limiting (5 requests per hour per key).
Swagger UI: Provides API documentation for testing.
Security & Configurations:
API keys are validated from appsettings.json.
HTTPS enforced using .NET dev-certs.
Frontend (React + Vite)
Pattern Used: Component-Based Architecture + Service Layer Abstraction
## Architecture:
Components: Weather.tsx handles user input and displays weather data.
Services: weatherService.ts abstracts API calls using Axios for separation of concerns.
State Management: useState + useEffect for managing weather data and errors.
Proxy Configuration: Vite is configured to proxy /api/weather requests to ASP.NET Core API.
Security & Optimizations:
Environment variables manage API URLs.
API errors are handled gracefully with proper user feedback.
## Deployment & Execution
PowerShell Script (run-project.ps1) automates running both backend and frontend.
CI/CD Ready: Can be containerized using Docker, deployed to Azure App Services or AWS.
## Advanced Considerations
Could extend to Microservices Architecture if scaling up.
Can integrate Redis Distributed Caching for API responses.
Supports JWT Authentication if user-based security is required.
## Overall: The project is highly modular, scalable, and follows best coding practices. 

