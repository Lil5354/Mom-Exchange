# ===================================================
# Script: Update Category Database
# Purpose: Run migrations and create Category tables
# ===================================================

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Category Database Update Script" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Method 1: Using Entity Framework Migrations
Write-Host "Method 1: Running EF Migrations..." -ForegroundColor Yellow
Write-Host "Command: Update-Database" -ForegroundColor Gray
Write-Host ""
Write-Host "Please run this command in Package Manager Console:" -ForegroundColor Green
Write-Host "  Update-Database" -ForegroundColor White
Write-Host ""

# Method 2: Using SQL Script
Write-Host "Method 2: Running SQL Script..." -ForegroundColor Yellow
$sqlScript = "SQL_Scripts\Create_Category_Tables.sql"

if (Test-Path $sqlScript) {
    Write-Host "Found SQL script: $sqlScript" -ForegroundColor Green
    Write-Host ""
    Write-Host "To run the script, use SQL Server Management Studio or run:" -ForegroundColor Green
    Write-Host "  sqlcmd -S (localdb)\MSSQLLocalDB -i $sqlScript" -ForegroundColor White
} else {
    Write-Host "SQL script not found: $sqlScript" -ForegroundColor Red
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "After running migration, test at:" -ForegroundColor Yellow
Write-Host "  https://localhost:44335/Admin/Category/Test" -ForegroundColor White
Write-Host "========================================" -ForegroundColor Cyan




