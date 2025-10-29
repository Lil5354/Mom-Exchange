# Update Database After Merge
# This script runs the SQL update script to fix the database schema

Write-Host "Updating database after merge..." -ForegroundColor Green

# Read the SQL script
$sqlScript = Get-Content "Simple_Database_Update.sql" -Raw

# Database connection string (update this to match your actual connection string)
$connectionString = "Data Source=localhost;Initial Catalog=MomExchange;Integrated Security=True;TrustServerCertificate=True"

try {
    # Create SQL connection
    $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    $connection.Open()
    
    # Execute the SQL script
    $command = New-Object System.Data.SqlClient.SqlCommand($sqlScript, $connection)
    $command.ExecuteNonQuery()
    
    Write-Host "Database updated successfully!" -ForegroundColor Green
    Write-Host "You can now try Google OAuth login again." -ForegroundColor Yellow
    
} catch {
    Write-Host "Error updating database: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Please run the SQL script manually in SQL Server Management Studio" -ForegroundColor Yellow
} finally {
    if ($connection.State -eq 'Open') {
        $connection.Close()
    }
}

Write-Host "Press any key to continue..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")