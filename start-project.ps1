# Kill any process running on the ports to avoid conflicts
$backendPort = 7250
$frontendPort = 5173

Write-Host "Stopping any process using ports $backendPort and $frontendPort..." -ForegroundColor Yellow
Get-Process | Where-Object { $_.Id -in (Get-NetTCPConnection -LocalPort $backendPort, $frontendPort -ErrorAction SilentlyContinue).OwningProcess } | Stop-Process -Force -ErrorAction SilentlyContinue

# Start Backend (ASP.NET Core API)
Write-Host "Starting Backend API..." -ForegroundColor Green
Start-Process -NoNewWindow -FilePath "powershell" -ArgumentList "cd ./OpenWeatherProxyApp.Server; dotnet run" 

# Wait for backend to start
Start-Sleep -Seconds 10

# Start Frontend (React Vite App)
Write-Host "Starting Frontend (React Vite)..." -ForegroundColor Green
Start-Process -NoNewWindow -FilePath "powershell" -ArgumentList "cd ./openweatherproxyapp.client; npm run dev"

# Wait for frontend to start
Start-Sleep -Seconds 5

# Open the browser for backend and frontend
Write-Host "Opening Backend Swagger UI..." -ForegroundColor Cyan
Start-Process "https://localhost:$backendPort/swagger/index.html"

Write-Host "Opening Frontend App..." -ForegroundColor Cyan
Start-Process "http://localhost:$frontendPort"

Write-Host "All services started successfully!" -ForegroundColor Green
