@echo off
set ASPNETCORE_ENVIRONMENT=Development
echo ======================================================================
echo           Starting ChatWithYourData ERP Services (.NET 10)
echo ======================================================================
echo.

echo [1/5] Starting InventoryService on http://localhost:5001/graphql ...
start "ChatWithYourData - InventoryService (Port 5001)" cmd /k "dotnet run --project src\Services\ChatWithYourData.InventoryService\ChatWithYourData.InventoryService.API\ChatWithYourData.InventoryService.API.csproj"

timeout /t 2 /nobreak >nul

echo [2/5] Starting SalesService on http://localhost:5002/graphql ...
start "ChatWithYourData - SalesService (Port 5002)" cmd /k "dotnet run --project src\Services\ChatWithYourData.SalesService\ChatWithYourData.SalesService.API\ChatWithYourData.SalesService.API.csproj"

timeout /t 2 /nobreak >nul

echo [3/5] Starting ProcurementService on http://localhost:5003/graphql ...
start "ChatWithYourData - ProcurementService (Port 5003)" cmd /k "dotnet run --project src\Services\ChatWithYourData.ProcurementService\ChatWithYourData.ProcurementService.API\ChatWithYourData.ProcurementService.API.csproj"

timeout /t 2 /nobreak >nul

echo [4/5] Starting FinanceService on http://localhost:5004/graphql ...
start "ChatWithYourData - FinanceService (Port 5004)" cmd /k "dotnet run --project src\Services\ChatWithYourData.FinanceService\ChatWithYourData.FinanceService.API\ChatWithYourData.FinanceService.API.csproj"

timeout /t 2 /nobreak >nul

echo [5/6] Starting Gateway on http://localhost:5000/graphql and http://localhost:5000/graphql/mcp ...
start "ChatWithYourData - Gateway (Port 5000)" cmd /k "dotnet run --project src\Gateway\ChatWithYourData.Gateway\ChatWithYourData.Gateway.csproj"

timeout /t 3 /nobreak >nul

echo [6/7] Starting ChatService (Microsoft Agents AI + AG-UI) on http://localhost:5005 ...
start "ChatWithYourData - ChatService (Port 5005)" cmd /k "dotnet run --project src\Services\ChatWithYourData.ChatService\ChatWithYourData.ChatService.API\ChatWithYourData.ChatService.API.csproj"

timeout /t 2 /nobreak >nul

echo [7/7] Starting Angular Frontend (CopilotKit AG-UI Client) on http://localhost:4200 ...
start "ChatWithYourData - Angular Frontend (Port 4200)" cmd /k "cd src\Frontend\ChatWithYourData.Web && npm start"

echo.
echo ======================================================================
echo All 7 services launched in separate terminal windows!
echo - Gateway (GraphQL + MCP): http://localhost:5000/graphql ^| http://localhost:5000/graphql/mcp
echo - Inventory Service:      http://localhost:5001/graphql
echo - Sales Service:          http://localhost:5002/graphql
echo - Procurement Service:    http://localhost:5003/graphql
echo - Finance Service:        http://localhost:5004/graphql
echo - ChatService (AG-UI):    http://localhost:5005/ag-ui ^| http://localhost:5005
echo - Angular Frontend:       http://localhost:4200
echo ======================================================================
echo.
pause
