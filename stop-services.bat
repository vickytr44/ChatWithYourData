@echo off
echo ======================================================================
echo           Stopping ChatWithYourData ERP Microservices
echo ======================================================================
echo.

echo Terminating running microservice processes...
taskkill /FI "WINDOWTITLE eq ChatWithYourData*" /T /F 2>nul
echo Done.
echo.
pause
