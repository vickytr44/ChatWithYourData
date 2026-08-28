@echo off
echo ======================================================================
echo           Starting ChatWithYourData ERP Microservices (.NET 10)
echo ======================================================================
echo.

echo [1/4] Starting InventoryService on http://localhost:5001/graphql ...
start "ChatWithYourData - InventoryService (Port 5001)" cmd /k "dotnet run --project src\Services\ChatWithYourData.InventoryService\ChatWithYourData.InventoryService.API\ChatWithYourData.InventoryService.API.csproj"

timeout /t 2 /nobreak >nul

echo [2/4] Starting SalesService on http://localhost:5002/graphql ...
start "ChatWithYourData - SalesService (Port 5002)" cmd /k "dotnet run --project src\Services\ChatWithYourData.SalesService\ChatWithYourData.SalesService.API\ChatWithYourData.SalesService.API.csproj"

timeout /t 2 /nobreak >nul

echo [3/4] Starting ProcurementService on http://localhost:5003/graphql ...
start "ChatWithYourData - ProcurementService (Port 5003)" cmd /k "dotnet run --project src\Services\ChatWithYourData.ProcurementService\ChatWithYourData.ProcurementService.API\ChatWithYourData.ProcurementService.API.csproj"

timeout /t 2 /nobreak >nul

echo [4/4] Starting FinanceService on http://localhost:5004/graphql ...
start "ChatWithYourData - FinanceService (Port 5004)" cmd /k "dotnet run --project src\Services\ChatWithYourData.FinanceService\ChatWithYourData.FinanceService.API\ChatWithYourData.FinanceService.API.csproj"

echo.
echo ======================================================================
echo All 4 microservices launched in separate terminal windows!
echo - Inventory Service:   http://localhost:5001/graphql
echo - Sales Service:       http://localhost:5002/graphql
echo - Procurement Service: http://localhost:5003/graphql
echo - Finance Service:     http://localhost:5004/graphql
echo ======================================================================
echo.
pause
