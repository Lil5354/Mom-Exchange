@echo off
echo ============================================
echo   KHOI DONG UNG DUNG MOM-EXCHANGE
echo ============================================
echo.
echo Checking Administrator privileges...
echo.

net session >nul 2>&1
if %errorLevel% == 0 (
    echo [OK] Running as Administrator
    echo.
    echo Starting IIS Express on port 44335...
    echo.
    cd /d "%~dp0"
    echo Working directory: %~dp0
    echo.
    "C:\Program Files\IIS Express\iisexpress.exe" /path:"%~dp0" /port:44335
) else (
    echo [ERROR] This script must be run as Administrator!
    echo.
    echo Please:
    echo 1. Right-click on START_APP.bat
    echo 2. Select "Run as administrator"
    echo.
    pause
    exit /b 1
)

