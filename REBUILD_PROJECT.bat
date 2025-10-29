@echo off
echo ========================================
echo REBUILDING MOM-EXCHANGE-WEB PROJECT
echo ========================================
echo.
echo Rebuilding project to compile new Admin Milk Donation Controller...
echo.

REM Navigate to project directory
cd /d "%~dp0"

REM Clean the project
echo [1/3] Cleaning project...
if exist "bin" (
    rd /s /q "bin"
    echo Cleaned bin folder
)
if exist "obj" (
    rd /s /q "obj"
    echo Cleaned obj folder
)

echo.
echo [2/3] Restoring NuGet packages...
nuget restore "B&M.sln"

echo.
echo [3/3] Building project...
msbuild "B&M.sln" /p:Configuration=Debug /t:Rebuild

echo.
echo ========================================
echo REBUILD COMPLETED!
echo ========================================
echo.
echo Now run the application:
echo   - Press F5 in Visual Studio
echo   - Or run: START_APP.bat
echo.
echo Then navigate to: http://localhost:44335/Admin/MilkDonation
echo.
pause



