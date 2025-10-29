# Update Email App Password
# This script helps you update the App Password in Web.config

param(
    [Parameter(Mandatory=$true)]
    [string]$AppPassword
)

Write-Host "Updating Email App Password..." -ForegroundColor Green

# Validate App Password format
if ($AppPassword.Length -ne 16) {
    Write-Host "❌ ERROR: App Password phải có đúng 16 ký tự!" -ForegroundColor Red
    Write-Host "Ví dụ: abcd efgh ijkl mnop" -ForegroundColor Yellow
    exit 1
}

# Remove spaces from App Password
$AppPassword = $AppPassword -replace '\s', ''

if ($AppPassword.Length -ne 16) {
    Write-Host "❌ ERROR: App Password sau khi xóa khoảng trắng phải có đúng 16 ký tự!" -ForegroundColor Red
    exit 1
}

try {
    # Read Web.config
    $webConfigPath = "Web.config"
    $xml = [xml](Get-Content $webConfigPath)
    
    # Update EmailPassword
    $emailPasswordNode = $xml.configuration.appSettings.add | Where-Object { $_.key -eq "EmailPassword" }
    if ($emailPasswordNode) {
        $emailPasswordNode.value = $AppPassword
        Write-Host "✅ Updated EmailPassword in Web.config" -ForegroundColor Green
    } else {
        Write-Host "❌ ERROR: Không tìm thấy EmailPassword trong Web.config" -ForegroundColor Red
        exit 1
    }
    
    # Save Web.config
    $xml.Save($webConfigPath)
    Write-Host "✅ Web.config đã được cập nhật thành công!" -ForegroundColor Green
    
    # Show current configuration
    Write-Host "`nCurrent Email Configuration:" -ForegroundColor Yellow
    $username = ($xml.configuration.appSettings.add | Where-Object { $_.key -eq "EmailUsername" }).value
    $fromEmail = ($xml.configuration.appSettings.add | Where-Object { $_.key -eq "EmailFromAddress" }).value
    Write-Host "  Username: $username"
    Write-Host "  From Email: $fromEmail"
    Write-Host "  App Password: $($AppPassword.Substring(0,4))****"
    
    Write-Host "`n✅ Hoàn thành! Bây giờ bạn có thể test email." -ForegroundColor Green
}
catch {
    Write-Host "❌ ERROR: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`nPress any key to continue..."
$Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown") | Out-Null
