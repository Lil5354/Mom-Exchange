# Setup SSL Certificate for IIS Express on Port 44335
# Run this script as Administrator

Write-Host "====================================" -ForegroundColor Cyan
Write-Host "IIS Express SSL Setup for Port 44335" -ForegroundColor Cyan
Write-Host "====================================" -ForegroundColor Cyan
Write-Host ""

# Step 1: Export certificate from CurrentUser
Write-Host "[1/5] Exporting certificate from CurrentUser store..." -ForegroundColor Yellow
$cert = Get-ChildItem -Path Cert:\CurrentUser\My | Where-Object {$_.Thumbprint -eq "365D07CDF43073CEE4B0B23A2F752258934F6260"}
if ($cert) {
    $pwd = ConvertTo-SecureString -String "temp123" -Force -AsPlainText
    Export-PfxCertificate -Cert $cert -FilePath "$env:TEMP\localhost.pfx" -Password $pwd -Force | Out-Null
    Write-Host "   OK Certificate exported" -ForegroundColor Green
} else {
    Write-Host "   ERROR Certificate not found in CurrentUser\My" -ForegroundColor Red
    exit 1
}

# Step 2: Import to LocalMachine
Write-Host "[2/5] Importing certificate to LocalMachine\My store..." -ForegroundColor Yellow
try {
    $pwd = ConvertTo-SecureString -String "temp123" -Force -AsPlainText
    Import-PfxCertificate -FilePath "$env:TEMP\localhost.pfx" -CertStoreLocation Cert:\LocalMachine\My -Password $pwd -Exportable | Out-Null
    Write-Host "   OK Certificate imported" -ForegroundColor Green
} catch {
    Write-Host "   ERROR Failed to import: $_" -ForegroundColor Red
    exit 1
}

# Step 3: Delete existing binding if exists
Write-Host "[3/5] Removing existing SSL binding (if any)..." -ForegroundColor Yellow
netsh http delete sslcert ipport=0.0.0.0:44335 2>$null | Out-Null
Write-Host "   OK Old binding removed" -ForegroundColor Green

# Step 4: Add new SSL binding
Write-Host "[4/5] Adding SSL certificate binding for port 44335..." -ForegroundColor Yellow
$result = netsh http add sslcert ipport=0.0.0.0:44335 certhash=365D07CDF43073CEE4B0B23A2F752258934F6260 appid="{214124cd-d05b-4309-9af9-9caa44b2b74a}" certstorename=MY 2>&1

if ($LASTEXITCODE -eq 0) {
    Write-Host "   OK SSL binding added successfully" -ForegroundColor Green
} else {
    Write-Host "   ERROR Failed to add binding: $result" -ForegroundColor Red
    exit 1
}

# Step 5: Verify binding
Write-Host "[5/5] Verifying SSL binding..." -ForegroundColor Yellow
$verify = netsh http show sslcert ipport=0.0.0.0:44335
if ($verify -match "44335") {
    Write-Host "   OK SSL binding verified" -ForegroundColor Green
    Write-Host ""
    Write-Host "====================================" -ForegroundColor Cyan
    Write-Host "SUCCESS! HTTPS is now configured for port 44335" -ForegroundColor Green
    Write-Host "====================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Next steps:" -ForegroundColor Yellow
    Write-Host "1. Close and reopen Visual Studio as Administrator" -ForegroundColor White
    Write-Host "2. Press F5 to run your project" -ForegroundColor White
    Write-Host "3. Browser will open: https://localhost:44335/" -ForegroundColor White
    Write-Host ""
} else {
    Write-Host "   ERROR Verification failed" -ForegroundColor Red
    exit 1
}

# Cleanup
Remove-Item "$env:TEMP\localhost.pfx" -Force -ErrorAction SilentlyContinue

Write-Host "Setup complete! Press any key to exit..."
Read-Host
