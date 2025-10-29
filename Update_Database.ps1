# Script to update database with Entity Framework migrations
Write-Host "Updating database with Entity Framework migrations..." -ForegroundColor Yellow

# Find Package Manager Console command
$migrationFiles = Get-ChildItem "Migrations" -Filter "*.cs" | Where-Object {$_.Name -match "^\d+_.*\.cs$"} | Sort-Object Name -Descending | Select-Object -First 1

if ($migrationFiles) {
    Write-Host "Latest migration: $($migrationFiles.Name)" -ForegroundColor Green
    Write-Host ""
    Write-Host "To update database, run this command in Visual Studio Package Manager Console:" -ForegroundColor Cyan
    Write-Host "Update-Database" -ForegroundColor White
    Write-Host ""
    Write-Host "Or open Visual Studio and:" -ForegroundColor Cyan
    Write-Host "1. Tools -> NuGet Package Manager -> Package Manager Console" -ForegroundColor White
    Write-Host "2. Run: Update-Database" -ForegroundColor White
} else {
    Write-Host "No migrations found!" -ForegroundColor Red
}





