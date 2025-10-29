# Setup SSL Certificate for IIS Express on Port 44335
# Run this script as Administrator
<<<<<<< HEAD
# Compatible with any Windows machine
=======
>>>>>>> Khoa

Write-Host "====================================" -ForegroundColor Cyan
Write-Host "IIS Express SSL Setup for Port 44335" -ForegroundColor Cyan
Write-Host "====================================" -ForegroundColor Cyan
Write-Host ""

<<<<<<< HEAD
# Check if running as Administrator
if (-NOT ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")) {
    Write-Host "ERROR: This script must be run as Administrator!" -ForegroundColor Red
    Write-Host "Right-click PowerShell and select 'Run as Administrator'" -ForegroundColor Yellow
    Read-Host "Press Enter to exit"
    exit 1
}

# Step 1: Find or create certificate
Write-Host "[1/6] Finding localhost certificate..." -ForegroundColor Yellow
$cert = Get-ChildItem -Path Cert:\CurrentUser\My | Where-Object {
    $_.Subject -like "*localhost*" -or 
    $_.Subject -like "*IIS Express*" -or
    $_.DnsNameList -contains "localhost"
} | Select-Object -First 1

if (-not $cert) {
    Write-Host "   No certificate found, creating new one..." -ForegroundColor Yellow
    try {
        $cert = New-SelfSignedCertificate -DnsName "localhost" -CertStoreLocation "Cert:\CurrentUser\My" -NotAfter (Get-Date).AddYears(1)
        Write-Host "   OK New certificate created: $($cert.Thumbprint)" -ForegroundColor Green
    } catch {
        Write-Host "   ERROR Failed to create certificate: $_" -ForegroundColor Red
        exit 1
    }
} else {
    Write-Host "   OK Found existing certificate: $($cert.Thumbprint)" -ForegroundColor Green
}

$thumbprint = $cert.Thumbprint
Write-Host "   Using thumbprint: $thumbprint" -ForegroundColor Cyan

# Step 2: Export certificate from CurrentUser
Write-Host "[2/6] Exporting certificate from CurrentUser store..." -ForegroundColor Yellow
try {
    $pwd = ConvertTo-SecureString -String "temp123" -Force -AsPlainText
    Export-PfxCertificate -Cert $cert -FilePath "$env:TEMP\localhost.pfx" -Password $pwd -Force | Out-Null
    Write-Host "   OK Certificate exported" -ForegroundColor Green
} catch {
    Write-Host "   ERROR Failed to export certificate: $_" -ForegroundColor Red
    exit 1
}

# Step 3: Import to LocalMachine
Write-Host "[3/6] Importing certificate to LocalMachine\My store..." -ForegroundColor Yellow
try {
    $pwd = ConvertTo-SecureString -String "temp123" -Force -AsPlainText
    Import-PfxCertificate -FilePath "$env:TEMP\localhost.pfx" -CertStoreLocation Cert:\LocalMachine\My -Password $pwd -Exportable | Out-Null
    Write-Host "   OK Certificate imported to LocalMachine" -ForegroundColor Green
=======
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
>>>>>>> Khoa
} catch {
    Write-Host "   ERROR Failed to import: $_" -ForegroundColor Red
    exit 1
}

<<<<<<< HEAD
# Step 4: Delete existing binding if exists
Write-Host "[4/6] Removing existing SSL binding (if any)..." -ForegroundColor Yellow
$deleteResult = netsh http delete sslcert ipport=0.0.0.0:44335 2>&1
if ($LASTEXITCODE -eq 0) {
    Write-Host "   OK Old binding removed" -ForegroundColor Green
} else {
    Write-Host "   INFO No existing binding found (this is OK)" -ForegroundColor Yellow
}

# Step 5: Add new SSL binding
Write-Host "[5/6] Adding SSL certificate binding for port 44335..." -ForegroundColor Yellow
$appId = "{214124cd-d05b-4309-9af9-9caa44b2b74a}"
$result = netsh http add sslcert ipport=0.0.0.0:44335 certhash=$thumbprint appid=$appId certstorename=MY 2>&1
=======
# Step 3: Delete existing binding if exists
Write-Host "[3/5] Removing existing SSL binding (if any)..." -ForegroundColor Yellow
netsh http delete sslcert ipport=0.0.0.0:44335 2>$null | Out-Null
Write-Host "   OK Old binding removed" -ForegroundColor Green

# Step 4: Add new SSL binding
Write-Host "[4/5] Adding SSL certificate binding for port 44335..." -ForegroundColor Yellow
$result = netsh http add sslcert ipport=0.0.0.0:44335 certhash=365D07CDF43073CEE4B0B23A2F752258934F6260 appid="{214124cd-d05b-4309-9af9-9caa44b2b74a}" certstorename=MY 2>&1
>>>>>>> Khoa

if ($LASTEXITCODE -eq 0) {
    Write-Host "   OK SSL binding added successfully" -ForegroundColor Green
} else {
    Write-Host "   ERROR Failed to add binding: $result" -ForegroundColor Red
<<<<<<< HEAD
    Write-Host "   Trying alternative method..." -ForegroundColor Yellow
    
    # Alternative method using netsh with different parameters
    $altResult = netsh http add sslcert ipport=0.0.0.0:44335 certhash=$thumbprint appid=$appId 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "   OK SSL binding added with alternative method" -ForegroundColor Green
    } else {
        Write-Host "   ERROR Alternative method also failed: $altResult" -ForegroundColor Red
        exit 1
    }
}

# Step 6: Verify binding
Write-Host "[6/6] Verifying SSL binding..." -ForegroundColor Yellow
$verify = netsh http show sslcert ipport=0.0.0.0:44335 2>&1
if ($verify -match "44335" -and $verify -match $thumbprint) {
    Write-Host "   OK SSL binding verified successfully" -ForegroundColor Green
=======
    exit 1
}

# Step 5: Verify binding
Write-Host "[5/5] Verifying SSL binding..." -ForegroundColor Yellow
$verify = netsh http show sslcert ipport=0.0.0.0:44335
if ($verify -match "44335") {
    Write-Host "   OK SSL binding verified" -ForegroundColor Green
>>>>>>> Khoa
    Write-Host ""
    Write-Host "====================================" -ForegroundColor Cyan
    Write-Host "SUCCESS! HTTPS is now configured for port 44335" -ForegroundColor Green
    Write-Host "====================================" -ForegroundColor Cyan
    Write-Host ""
<<<<<<< HEAD
    Write-Host "Certificate Details:" -ForegroundColor Yellow
    Write-Host "  Thumbprint: $thumbprint" -ForegroundColor White
    Write-Host "  Subject: $($cert.Subject)" -ForegroundColor White
    Write-Host "  Valid Until: $($cert.NotAfter)" -ForegroundColor White
    Write-Host ""
=======
>>>>>>> Khoa
    Write-Host "Next steps:" -ForegroundColor Yellow
    Write-Host "1. Close and reopen Visual Studio as Administrator" -ForegroundColor White
    Write-Host "2. Press F5 to run your project" -ForegroundColor White
    Write-Host "3. Browser will open: https://localhost:44335/" -ForegroundColor White
<<<<<<< HEAD
    Write-Host "4. Accept the security warning (certificate is self-signed)" -ForegroundColor White
    Write-Host ""
} else {
    Write-Host "   ERROR Verification failed" -ForegroundColor Red
    Write-Host "   Debug info:" -ForegroundColor Yellow
    Write-Host "   $verify" -ForegroundColor White
=======
    Write-Host ""
} else {
    Write-Host "   ERROR Verification failed" -ForegroundColor Red
>>>>>>> Khoa
    exit 1
}

# Cleanup
<<<<<<< HEAD
Write-Host "Cleaning up temporary files..." -ForegroundColor Yellow
Remove-Item "$env:TEMP\localhost.pfx" -Force -ErrorAction SilentlyContinue
Write-Host "   OK Cleanup completed" -ForegroundColor Green

Write-Host ""
Write-Host "Setup complete! Press any key to exit..."
Read-Host
=======
Remove-Item "$env:TEMP\localhost.pfx" -Force -ErrorAction SilentlyContinue

Write-Host "Setup complete! Press any key to exit..."
Read-Host
>>>>>>> Khoa
