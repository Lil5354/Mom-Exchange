# Script to start IIS Express as Administrator
# Run this script as Administrator to fix Access Denied error

param(
    [string]$ProjectPath = "C:\Users\Admin\Downloads\Mom-Exchange-Web",
    [int]$Port = 44335
)

Write-Host "=== STARTING IIS EXPRESS AS ADMINISTRATOR ===" -ForegroundColor Green
Write-Host "Project Path: $ProjectPath" -ForegroundColor Yellow
Write-Host "Port: $Port" -ForegroundColor Yellow

# Check if running as Administrator
$currentPrincipal = New-Object Security.Principal.WindowsPrincipal([Security.Principal.WindowsIdentity]::GetCurrent())
$isAdmin = $currentPrincipal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)

if (-not $isAdmin) {
    Write-Host "ERROR: This script must be run as Administrator!" -ForegroundColor Red
    Write-Host "Right-click PowerShell -> Run as Administrator" -ForegroundColor Yellow
    Read-Host "Press Enter to exit"
    exit 1
}

# Check if project path exists
if (-not (Test-Path $ProjectPath)) {
    Write-Host "ERROR: Project path not found: $ProjectPath" -ForegroundColor Red
    Read-Host "Press Enter to exit"
    exit 1
}

# Check if IIS Express exists
$iisExpressPath = "${env:ProgramFiles}\IIS Express\iisexpress.exe"
if (-not (Test-Path $iisExpressPath)) {
    Write-Host "ERROR: IIS Express not found at: $iisExpressPath" -ForegroundColor Red
    Read-Host "Press Enter to exit"
    exit 1
}

Write-Host "Starting IIS Express..." -ForegroundColor Green

try {
    # Change to project directory
    Set-Location $ProjectPath
    
    # Start IIS Express
    & $iisExpressPath /path:"$ProjectPath" /port:$Port
}
catch {
    Write-Host "ERROR: Failed to start IIS Express: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "IIS Express stopped." -ForegroundColor Yellow
Read-Host "Press Enter to exit"


