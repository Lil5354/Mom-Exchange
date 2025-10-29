# Test Email Configuration
# This script tests the email configuration

Write-Host "Testing Email Configuration..." -ForegroundColor Green

# Read Web.config to get email settings
$webConfigPath = "Web.config"
$xml = [xml](Get-Content $webConfigPath)

# Extract email settings
$smtpHost = ($xml.configuration.appSettings.add | Where-Object { $_.key -eq "EmailSmtpHost" }).value
$smtpPort = [int]($xml.configuration.appSettings.add | Where-Object { $_.key -eq "EmailSmtpPort" }).value
$username = ($xml.configuration.appSettings.add | Where-Object { $_.key -eq "EmailUsername" }).value
$password = ($xml.configuration.appSettings.add | Where-Object { $_.key -eq "EmailPassword" }).value
$enableSsl = [bool]::Parse(($xml.configuration.appSettings.add | Where-Object { $_.key -eq "EmailEnableSsl" }).value)
$fromEmail = ($xml.configuration.appSettings.add | Where-Object { $_.key -eq "EmailFromAddress" }).value
$fromName = ($xml.configuration.appSettings.add | Where-Object { $_.key -eq "EmailFromName" }).value

Write-Host "Email Configuration:" -ForegroundColor Yellow
Write-Host "  SMTP Host: $smtpHost"
Write-Host "  SMTP Port: $smtpPort"
Write-Host "  Username: $username"
Write-Host "  Password: $($password.Substring(0,4))****" 
Write-Host "  Enable SSL: $enableSsl"
Write-Host "  From Email: $fromEmail"
Write-Host "  From Name: $fromName"

# Check if password is still placeholder
if ($password -eq "YOUR_APP_PASSWORD_HERE") {
    Write-Host "`n❌ ERROR: App Password chưa được cấu hình!" -ForegroundColor Red
    Write-Host "Vui lòng:" -ForegroundColor Yellow
    Write-Host "1. Lấy App Password từ Gmail (16 ký tự)" -ForegroundColor Yellow
    Write-Host "2. Thay thế 'YOUR_APP_PASSWORD_HERE' trong Web.config" -ForegroundColor Yellow
    Write-Host "3. Chạy lại script này" -ForegroundColor Yellow
    exit 1
}

# Test email sending
try {
    Write-Host "`nSending test email..." -ForegroundColor Green
    
    # Create email message
    $message = New-Object System.Net.Mail.MailMessage
    $message.From = New-Object System.Net.Mail.MailAddress($fromEmail, $fromName)
    $message.To.Add($username)  # Send to self
    $message.Subject = "Test Email - MomExchange System"
    $message.Body = @"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; }
        .container { max-width: 600px; margin: 0 auto; padding: 20px; background: #f9f9f9; }
        .content { background: white; padding: 30px; border-radius: 10px; text-align: center; }
        .success { color: #27ae60; font-size: 48px; }
    </style>
</head>
<body>
    <div class='container'>
        <div class='content'>
            <div class='success'>✅</div>
            <h2>Test Email thành công!</h2>
            <p>Cấu hình email của bạn đang hoạt động tốt.</p>
            <p><strong>Thời gian:</strong> $(Get-Date -Format 'dd/MM/yyyy HH:mm:ss')</p>
            <hr>
            <p style='font-size: 12px; color: #666;'>Email được gửi từ MomExchange System</p>
        </div>
    </div>
</body>
</html>
"@
    $message.IsBodyHtml = $true

    # Create SMTP client
    $smtpClient = New-Object System.Net.Mail.SmtpClient($smtpHost, $smtpPort)
    $smtpClient.Credentials = New-Object System.Net.NetworkCredential($username, $password)
    $smtpClient.EnableSsl = $enableSsl
    $smtpClient.Timeout = 30000

    # Send email
    $smtpClient.Send($message)
    $smtpClient.Dispose()

    Write-Host "✅ Email test thành công!" -ForegroundColor Green
    Write-Host "Kiểm tra hộp thư của $username" -ForegroundColor Yellow
}
catch {
    Write-Host "❌ Lỗi gửi email: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Chi tiết: $($_.Exception.InnerException.Message)" -ForegroundColor Red
}

Write-Host "`nPress any key to continue..."
$Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown") | Out-Null
