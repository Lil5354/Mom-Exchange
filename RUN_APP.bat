@echo off
echo ============================================
echo   KHOI DONG UNG DUNG MOM-EXCHANGE
echo ============================================
echo.

REM Check Administrator privileges
net session >nul 2>&1
if %errorLevel% neq 0 (
    echo [ERROR] Phai chay voi quyen Administrator!
    echo.
    echo Please right-click and select "Run as administrator"
    echo.
    pause
    exit /b 1
)

echo [OK] Running as Administrator
echo.

REM Set project directory
set PROJECT_DIR=%~dp0
cd /d "%PROJECT_DIR%"

echo Project directory: %PROJECT_DIR%
echo.

REM Check if applicationhost.config exists
if not exist ".vs\config\applicationhost.config" (
    echo [ERROR] Missing applicationhost.config file!
    echo Please run this from Visual Studio first to generate config.
    pause
    exit /b 1
)

echo Starting IIS Express...
echo.

"C:\Program Files\IIS Express\iisexpress.exe" /config:"%PROJECT_DIR%.vs\config\applicationhost.config" /site:"MomExchangeWeb" /systray:false

pause



