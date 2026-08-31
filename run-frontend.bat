@echo off
echo ======================================================================
echo       Starting ChatWithYourData Angular Frontend Client
echo ======================================================================
echo.
echo Launching Angular dev server on http://localhost:4200 ...
echo Connecting to AG-UI Backend on http://localhost:5005/ag-ui
echo.

cd /d "%~dp0src\Frontend\ChatWithYourData.Web"
npm start
