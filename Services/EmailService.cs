// File: Services/EmailService.cs
using System;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Configuration;

namespace B_M.Services
{
    public class EmailService
    {
        private readonly string _smtpHost;
        private readonly int _smtpPort;
        private readonly string _username;
        private readonly string _password;
        private readonly bool _enableSsl;
        private readonly string _fromEmail;
        private readonly string _fromName;

        public EmailService()
        {
            // Load settings from Web.config or use defaults
            _smtpHost = ConfigurationManager.AppSettings["EmailSmtpHost"] ?? "smtp.gmail.com";
            _smtpPort = int.Parse(ConfigurationManager.AppSettings["EmailSmtpPort"] ?? "587");
            _username = ConfigurationManager.AppSettings["EmailUsername"] ?? "";
            _password = ConfigurationManager.AppSettings["EmailPassword"] ?? "";
            _enableSsl = bool.Parse(ConfigurationManager.AppSettings["EmailEnableSsl"] ?? "true");
            _fromEmail = ConfigurationManager.AppSettings["EmailFromAddress"] ?? "noreply@momexchange.com";
            _fromName = ConfigurationManager.AppSettings["EmailFromName"] ?? "MomExchange System";
        }

        public EmailService(string smtpHost, int smtpPort, string username, string password, 
            bool enableSsl, string fromEmail, string fromName)
        {
            _smtpHost = smtpHost;
            _smtpPort = smtpPort;
            _username = username;
            _password = password;
            _enableSsl = enableSsl;
            _fromEmail = fromEmail;
            _fromName = fromName;
        }

        public EmailResult SendEmail(string toEmail, string subject, string body, bool isHtml = true)
        {
            try
            {
                // Validate email settings
                if (string.IsNullOrEmpty(_smtpHost) || string.IsNullOrEmpty(_username) || string.IsNullOrEmpty(_password))
                {
                    return new EmailResult
                    {
                        Success = false,
                        Message = "Email chưa được cấu hình. Vui lòng cấu hình email trong phần Cài đặt."
                    };
                }

                // Force TLS 1.2 for Gmail (required)
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                
                // Trust all certificates (for development/testing only - remove in production if you have valid certs)
                ServicePointManager.ServerCertificateValidationCallback = 
                    delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) 
                    { return true; };

                System.Diagnostics.Debug.WriteLine($"[EmailService] Sending email - SSL: {_enableSsl}, Host: {_smtpHost}:{_smtpPort}");

                using (var message = new MailMessage())
                {
                    message.From = new MailAddress(_fromEmail, _fromName);
                    message.To.Add(new MailAddress(toEmail));
                    message.Subject = subject;
                    message.Body = body;
                    message.IsBodyHtml = isHtml;
                    message.Priority = MailPriority.Normal;

                    using (var client = new SmtpClient(_smtpHost, _smtpPort))
                    {
                        client.Credentials = new NetworkCredential(_username, _password);
                        client.EnableSsl = _enableSsl;
                        client.Timeout = 30000; // 30 seconds
                        client.DeliveryMethod = SmtpDeliveryMethod.Network;
                        client.UseDefaultCredentials = false;

                        client.Send(message);
                    }
                }

                return new EmailResult
                {
                    Success = true,
                    Message = "Email đã được gửi thành công."
                };
            }
            catch (SmtpException ex)
            {
                System.Diagnostics.Debug.WriteLine($"SMTP Error: {ex.Message}");
                return new EmailResult
                {
                    Success = false,
                    Message = $"Lỗi gửi email: {ex.Message}"
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Email Error: {ex.Message}");
                return new EmailResult
                {
                    Success = false,
                    Message = $"Lỗi không xác định: {ex.Message}"
                };
            }
        }

        public EmailResult SendWelcomeEmail(string toEmail, string fullName, string username, string temporaryPassword = null)
        {
            var subject = "Chào mừng đến với MomExchange!";
            var body = GetWelcomeEmailTemplate(fullName, username, temporaryPassword);
            return SendEmail(toEmail, subject, body, true);
        }

        public EmailResult SendPasswordResetEmail(string toEmail, string fullName, string resetToken, string resetUrl)
        {
            var subject = "Yêu cầu đặt lại mật khẩu - MomExchange";
            var body = GetPasswordResetEmailTemplate(fullName, resetToken, resetUrl);
            return SendEmail(toEmail, subject, body, true);
        }

        public EmailResult SendTestEmail(string toEmail)
        {
            var subject = "Test Email - MomExchange System";
            var body = GetTestEmailTemplate();
            return SendEmail(toEmail, subject, body, true);
        }

        /// <summary>
        /// Test email connection with custom settings (for Settings page)
        /// </summary>
        public EmailResult TestEmailConnection(string smtpHost, int smtpPort, string username, 
            string password, bool enableSsl, string fromEmail, string fromName)
        {
            try
            {
                // Clean and validate inputs
                smtpHost = smtpHost?.Trim();
                username = username?.Trim();
                password = password?.Trim();
                fromEmail = fromEmail?.Trim();
                fromName = fromName?.Trim();
                
                // Debug logging (don't log full password)
                System.Diagnostics.Debug.WriteLine($"[TestEmailConnection] Host: {smtpHost}, Port: {smtpPort}, SSL: {enableSsl}");
                System.Diagnostics.Debug.WriteLine($"[TestEmailConnection] Username: {username}, PasswordLength: {password?.Length ?? 0}");
                System.Diagnostics.Debug.WriteLine($"[TestEmailConnection] FromEmail: {fromEmail}, FromName: {fromName}");
                
                // Validate inputs
                if (string.IsNullOrEmpty(smtpHost))
                {
                    return new EmailResult { Success = false, Message = "SMTP Host không được để trống." };
                }
                if (string.IsNullOrEmpty(username))
                {
                    return new EmailResult { Success = false, Message = "Username không được để trống." };
                }
                if (string.IsNullOrEmpty(password))
                {
                    return new EmailResult { Success = false, Message = "Password không được để trống." };
                }
                if (string.IsNullOrEmpty(fromEmail))
                {
                    return new EmailResult { Success = false, Message = "From Email không được để trống." };
                }
                if (string.IsNullOrEmpty(fromName))
                {
                    return new EmailResult { Success = false, Message = "From Name không được để trống." };
                }
                
                // Validate email format
                try
                {
                    var addr = new System.Net.Mail.MailAddress(fromEmail);
                    if (addr.Address != fromEmail)
                    {
                        return new EmailResult { Success = false, Message = "From Email không hợp lệ." };
                    }
                }
                catch
                {
                    return new EmailResult { Success = false, Message = "From Email không đúng định dạng." };
                }

                // Special validation for Gmail
                if (smtpHost.ToLower().Contains("gmail.com"))
                {
                    System.Diagnostics.Debug.WriteLine("[TestEmailConnection] Gmail detected - applying Gmail-specific validations");
                    
                    if (!enableSsl)
                    {
                        return new EmailResult
                        {
                            Success = false,
                            Message = "Gmail yêu cầu bật SSL/TLS. Vui lòng check vào ô 'Bật SSL/TLS'."
                        };
                    }
                    if (smtpPort != 587 && smtpPort != 465)
                    {
                        return new EmailResult
                        {
                            Success = false,
                            Message = $"Gmail yêu cầu Port 587 (TLS) hoặc 465 (SSL). Port hiện tại: {smtpPort}"
                        };
                    }
                    if (password.Length != 16)
                    {
                        System.Diagnostics.Debug.WriteLine($"[TestEmailConnection] Warning: Gmail App Password should be 16 chars, got {password.Length}");
                        // Don't fail, just warn
                    }
                    // Check if password contains spaces (common mistake)
                    if (password.Contains(" "))
                    {
                        return new EmailResult
                        {
                            Success = false,
                            Message = "Password chứa khoảng trắng. Gmail App Password không có khoảng trắng. Vui lòng xóa khoảng trắng và thử lại."
                        };
                    }
                }

                System.Diagnostics.Debug.WriteLine("[TestEmailConnection] Creating test email service...");
                
                // Create test email service with provided settings
                var testEmailService = new EmailService(smtpHost, smtpPort, username, password, enableSsl, fromEmail, fromName);
                
                System.Diagnostics.Debug.WriteLine("[TestEmailConnection] Sending test email...");
                
                // Send test email to the from email address
                var result = testEmailService.SendTestEmail(fromEmail);
                
                System.Diagnostics.Debug.WriteLine($"[TestEmailConnection] Result: Success={result.Success}, Message={result.Message}");
                
                if (result.Success)
                {
                    return new EmailResult
                    {
                        Success = true,
                        Message = $"✅ Kết nối thành công! Email test đã được gửi tới {fromEmail}. Vui lòng kiểm tra hộp thư của bạn."
                    };
                }
                else
                {
                    return new EmailResult
                    {
                        Success = false,
                        Message = "❌ " + result.Message
                    };
                }
            }
            catch (SmtpException ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SMTP Test Error] Message: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[SMTP Test Error] StatusCode: {ex.StatusCode}");
                System.Diagnostics.Debug.WriteLine($"[SMTP Test Error] StackTrace: {ex.StackTrace}");
                
                string errorMessage = "❌ Kết nối SMTP thất bại. ";
                
                // Parse specific SMTP errors
                if (ex.Message.Contains("5.7.0") || ex.Message.Contains("STARTTLS") || ex.Message.Contains("secure connection"))
                {
                    errorMessage += "Gmail yêu cầu kết nối bảo mật (SSL/TLS). Vui lòng CHECK vào ô 'Bật SSL/TLS'.";
                }
                else if (ex.Message.Contains("5.7.8") || ex.Message.Contains("5.7.14") || 
                         ex.Message.Contains("Username and Password not accepted") ||
                         ex.Message.Contains("Authentication Required") ||
                         ex.Message.Contains("Invalid credentials"))
                {
                    errorMessage += "Username hoặc Password không đúng. ";
                    errorMessage += "Nếu dùng Gmail: (1) Đảm bảo đã bật 2-Step Verification, ";
                    errorMessage += "(2) Sử dụng App Password 16 ký tự (không có khoảng trắng), ";
                    errorMessage += "(3) Username phải là email đầy đủ (ví dụ: user@gmail.com).";
                }
                else if (ex.Message.Contains("Unable to connect") || ex.Message.Contains("No connection could be made"))
                {
                    errorMessage += "Không thể kết nối tới SMTP server. Kiểm tra: (1) SMTP Host đúng, (2) SMTP Port đúng, (3) Firewall/Antivirus không chặn.";
                }
                else if (ex.Message.Contains("timed out") || ex.Message.Contains("timeout"))
                {
                    errorMessage += "Kết nối timeout. Kiểm tra kết nối internet hoặc firewall.";
                }
                else
                {
                    errorMessage += "Chi tiết: " + ex.Message;
                }
                
                return new EmailResult
                {
                    Success = false,
                    Message = errorMessage
                };
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Socket Error] {ex.Message}");
                return new EmailResult
                {
                    Success = false,
                    Message = $"❌ Lỗi kết nối mạng: {ex.Message}. Kiểm tra internet hoặc firewall."
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Email Test Error] Type: {ex.GetType().Name}");
                System.Diagnostics.Debug.WriteLine($"[Email Test Error] Message: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[Email Test Error] StackTrace: {ex.StackTrace}");
                
                return new EmailResult
                {
                    Success = false,
                    Message = $"❌ Lỗi không xác định: {ex.Message}"
                };
            }
        }

        private string GetWelcomeEmailTemplate(string fullName, string username, string temporaryPassword)
        {
            var passwordInfo = string.IsNullOrEmpty(temporaryPassword) 
                ? "<p>Bạn có thể đăng nhập bằng mật khẩu đã tạo khi đăng ký.</p>"
                : $@"
                    <p><strong>Mật khẩu tạm thời:</strong> <code style='background: #f4f4f4; padding: 5px 10px; border-radius: 4px; font-size: 16px;'>{temporaryPassword}</code></p>
                    <p style='color: #e74c3c;'><strong>⚠️ Quan trọng:</strong> Vui lòng đổi mật khẩu ngay sau khi đăng nhập lần đầu.</p>";

            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ background: #fff; padding: 30px; border: 1px solid #e0e0e0; border-top: none; }}
        .footer {{ background: #f8f9fa; padding: 20px; text-align: center; font-size: 12px; color: #666; border-radius: 0 0 10px 10px; }}
        .button {{ display: inline-block; background: #667eea; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
        .info-box {{ background: #e8f4f8; border-left: 4px solid #3498db; padding: 15px; margin: 20px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1 style='margin: 0;'>👋 Chào mừng đến với MomExchange!</h1>
        </div>
        <div class='content'>
            <p>Xin chào <strong>{fullName}</strong>,</p>
            <p>Chúc mừng! Tài khoản của bạn đã được tạo thành công trên hệ thống <strong>MomExchange</strong>.</p>
            
            <div class='info-box'>
                <p style='margin: 0;'><strong>📧 Email:</strong> {username}</p>
                {passwordInfo}
            </div>

            <p>MomExchange là nền tảng trao đổi và chia sẻ dành cho các bà mẹ. Tại đây bạn có thể:</p>
            <ul>
                <li>Chia sẻ kinh nghiệm nuôi dạy con</li>
                <li>Trao đổi đồ dùng cho bé</li>
                <li>Kết nối với cộng đồng mẹ bỉm</li>
                <li>Hiến tặng sữa mẹ</li>
            </ul>

            <div style='text-align: center;'>
                <a href='https://momexchange.com/login' class='button'>Đăng nhập ngay</a>
            </div>

            <p>Nếu bạn có bất kỳ câu hỏi nào, đừng ngại liên hệ với chúng tôi qua email: <a href='mailto:support@momexchange.com'>support@momexchange.com</a></p>
            
            <p>Trân trọng,<br><strong>Đội ngũ MomExchange</strong></p>
        </div>
        <div class='footer'>
            <p>&copy; {DateTime.Now.Year} MomExchange. All rights reserved.</p>
            <p>Email này được gửi tự động, vui lòng không trả lời.</p>
        </div>
    </div>
</body>
</html>";
        }

        private string GetPasswordResetEmailTemplate(string fullName, string resetToken, string resetUrl)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: #e74c3c; color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ background: #fff; padding: 30px; border: 1px solid #e0e0e0; border-top: none; }}
        .footer {{ background: #f8f9fa; padding: 20px; text-align: center; font-size: 12px; color: #666; border-radius: 0 0 10px 10px; }}
        .button {{ display: inline-block; background: #e74c3c; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
        .warning {{ background: #fff3cd; border-left: 4px solid #ffc107; padding: 15px; margin: 20px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1 style='margin: 0;'>🔐 Đặt lại mật khẩu</h1>
        </div>
        <div class='content'>
            <p>Xin chào <strong>{fullName}</strong>,</p>
            <p>Chúng tôi nhận được yêu cầu đặt lại mật khẩu cho tài khoản của bạn.</p>
            
            <div style='text-align: center;'>
                <a href='{resetUrl}?token={resetToken}' class='button'>Đặt lại mật khẩu</a>
            </div>

            <div class='warning'>
                <p style='margin: 0;'><strong>⚠️ Lưu ý:</strong></p>
                <ul style='margin: 10px 0 0 0;'>
                    <li>Link này chỉ có hiệu lực trong 24 giờ</li>
                    <li>Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này</li>
                </ul>
            </div>

            <p>Trân trọng,<br><strong>Đội ngũ MomExchange</strong></p>
        </div>
        <div class='footer'>
            <p>&copy; {DateTime.Now.Year} MomExchange. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
        }

        private string GetTestEmailTemplate()
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; background: #f9f9f9; }}
        .content {{ background: white; padding: 30px; border-radius: 10px; text-align: center; }}
        .success {{ color: #27ae60; font-size: 48px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='content'>
            <div class='success'>✅</div>
            <h2>Test Email thành công!</h2>
            <p>Cấu hình email của bạn đang hoạt động tốt.</p>
            <p><strong>Thời gian:</strong> {DateTime.Now:dd/MM/yyyy HH:mm:ss}</p>
            <hr>
            <p style='font-size: 12px; color: #666;'>Email được gửi từ MomExchange System</p>
        </div>
    </div>
</body>
</html>";
        }

        /// <summary>
        /// Gửi email đơn giản (Plain Text) - cho Library Management compatibility
        /// </summary>
        public EmailResult SendSimpleEmail(string toEmail, string subject, string body)
        {
            return SendEmail(toEmail, subject, body, isHtml: false);
        }

        /// <summary>
        /// Gửi HTML Email - cho Library Management compatibility
        /// </summary>
        public EmailResult SendHtmlEmail(string toEmail, string subject, string htmlContent)
        {
            return SendEmail(toEmail, subject, htmlContent, isHtml: true);
        }

        /// <summary>
        /// Gửi email với file đính kèm (như Library Management)
        /// </summary>
        public EmailResult SendEmailWithAttachment(string toEmail, string subject, string body, 
            bool isHtml, string attachmentFilePath, string attachmentFileName = null)
        {
            try
            {
                // Validate email settings
                if (string.IsNullOrEmpty(_smtpHost) || string.IsNullOrEmpty(_username) || string.IsNullOrEmpty(_password))
                {
                    return new EmailResult
                    {
                        Success = false,
                        Message = "Email chưa được cấu hình. Vui lòng cấu hình email trong phần Cài đặt."
                    };
                }

                // Force TLS 1.2
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                ServicePointManager.ServerCertificateValidationCallback = 
                    delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) 
                    { return true; };

                using (var message = new MailMessage())
                {
                    message.From = new MailAddress(_fromEmail, _fromName);
                    message.To.Add(new MailAddress(toEmail));
                    message.Subject = subject;
                    message.Body = body;
                    message.IsBodyHtml = isHtml;
                    message.Priority = MailPriority.Normal;

                    // Xử lý tệp đính kèm
                    if (!string.IsNullOrWhiteSpace(attachmentFilePath) && System.IO.File.Exists(attachmentFilePath))
                    {
                        var attachment = new System.Net.Mail.Attachment(attachmentFilePath);
                        
                        // Nếu có tên tùy chỉnh, đổi tên file
                        if (!string.IsNullOrWhiteSpace(attachmentFileName))
                        {
                            attachment.Name = attachmentFileName;
                        }
                        
                        message.Attachments.Add(attachment);
                        System.Diagnostics.Debug.WriteLine($"[EmailService] Attached file: {attachmentFilePath}");
                    }
                    else if (!string.IsNullOrWhiteSpace(attachmentFilePath))
                    {
                        System.Diagnostics.Debug.WriteLine($"[EmailService] Warning: Attachment file not found: {attachmentFilePath}");
                    }

                    using (var client = new SmtpClient(_smtpHost, _smtpPort))
                    {
                        client.Credentials = new NetworkCredential(_username, _password);
                        client.EnableSsl = _enableSsl;
                        client.Timeout = 30000;
                        client.DeliveryMethod = SmtpDeliveryMethod.Network;
                        client.UseDefaultCredentials = false;

                        client.Send(message);
                    }
                }

                return new EmailResult
                {
                    Success = true,
                    Message = "Email với tệp đính kèm đã được gửi thành công."
                };
            }
            catch (SmtpException ex)
            {
                System.Diagnostics.Debug.WriteLine($"SMTP Error (with attachment): {ex.Message}");
                return new EmailResult
                {
                    Success = false,
                    Message = $"Lỗi gửi email: {ex.Message}"
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Email Error (with attachment): {ex.Message}");
                return new EmailResult
                {
                    Success = false,
                    Message = $"Lỗi không xác định: {ex.Message}"
                };
            }
        }
    }

    public class EmailResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }
}

