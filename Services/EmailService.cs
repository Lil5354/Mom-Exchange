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
            
            // Debug logging
            System.Diagnostics.Debug.WriteLine("=== EMAIL SERVICE CONSTRUCTOR ===");
            System.Diagnostics.Debug.WriteLine($"SMTP Host: {_smtpHost}");
            System.Diagnostics.Debug.WriteLine($"SMTP Port: {_smtpPort}");
            System.Diagnostics.Debug.WriteLine($"Username: {_username}");
            System.Diagnostics.Debug.WriteLine($"Password Length: {(_password?.Length ?? 0)}");
            System.Diagnostics.Debug.WriteLine($"Enable SSL: {_enableSsl}");
            System.Diagnostics.Debug.WriteLine($"From Email: {_fromEmail}");
            System.Diagnostics.Debug.WriteLine($"From Name: {_fromName}");
            System.Diagnostics.Debug.WriteLine("================================");
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
                
                // Trust all certificates (for development/testing only)
                ServicePointManager.ServerCertificateValidationCallback = 
                    delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) 
                    { 
                        System.Diagnostics.Debug.WriteLine($"Certificate validation: {sslPolicyErrors}");
                        return true; 
                    };

                System.Diagnostics.Debug.WriteLine($"[EmailService] Sending email - SSL: {_enableSsl}, Host: {_smtpHost}:{_smtpPort}");

                using (var message = new MailMessage())
                {
                    message.From = new MailAddress(_fromEmail, _fromName);
                    message.To.Add(new MailAddress(toEmail));
                    message.Subject = subject;
                    message.Body = body;
                    message.IsBodyHtml = isHtml;
                    message.Priority = MailPriority.Normal;

                    using (var client = new SmtpClient())
                    {
                        client.Host = _smtpHost;
                        client.Port = _smtpPort;
                        client.EnableSsl = _enableSsl;
                        client.UseDefaultCredentials = false;
                        client.Credentials = new NetworkCredential(_username, _password);
                        client.Timeout = 30000;
                        client.DeliveryMethod = SmtpDeliveryMethod.Network;
                        
                        // Additional settings for Gmail
                        System.Diagnostics.Debug.WriteLine($"Attempting to send email to {toEmail}...");
                        System.Diagnostics.Debug.WriteLine($"SMTP Host: {_smtpHost}, Port: {_smtpPort}, SSL: {_enableSsl}");
                        System.Diagnostics.Debug.WriteLine($"Username: {_username}");
                        
                        client.Send(message);
                        
                        System.Diagnostics.Debug.WriteLine($"Email sent successfully to {toEmail}");
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
                System.Diagnostics.Debug.WriteLine($"Status Code: {ex.StatusCode}");
                
                string errorMessage = "Lỗi gửi email: ";
                
                if (ex.Message.Contains("authentication") || ex.Message.Contains("not authenticated"))
                {
                    errorMessage += "Xác thực thất bại. Vui lòng kiểm tra lại Username và App Password trong Web.config. " +
                                   "App Password phải là 16 ký tự không có khoảng trắng và vẫn còn hiệu lực.";
                }
                else if (ex.Message.Contains("secure connection"))
                {
                    errorMessage += "Yêu cầu kết nối bảo mật. Vui lòng đảm bảo EnableSsl = true.";
                }
                else
                {
                    errorMessage += ex.Message;
                }
                
                return new EmailResult
                {
                    Success = false,
                    Message = errorMessage
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Email Error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
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
                    <p><strong>🔑 Mật khẩu tạm thời của bạn:</strong></p>
                    <div style='background: linear-gradient(135deg, #fff5f8 0%, #ffe0eb 100%); border: 2px solid #f8a5c2; padding: 15px; border-radius: 8px; text-align: center; margin: 15px 0;'>
                        <code style='background: white; padding: 10px 20px; border-radius: 5px; font-size: 18px; font-weight: bold; color: #e91e63; letter-spacing: 2px; border: 2px dashed #f8a5c2;'>{temporaryPassword}</code>
                    </div>
                    <p style='color: #e91e63; background: #fff5f8; padding: 12px; border-radius: 6px; border-left: 4px solid #f8a5c2;'><strong>⚠️ Quan trọng:</strong> Vui lòng đổi mật khẩu ngay sau khi đăng nhập lần đầu để bảo mật tài khoản!</p>";

            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; line-height: 1.6; color: #333; background-color: #f5f5f5; }}
        .container {{ max-width: 600px; margin: 30px auto; background: white; box-shadow: 0 4px 15px rgba(0,0,0,0.1); }}
        .header {{ background: linear-gradient(135deg, #ff8fab 0%, #fc8dc7 50%, #f093fb 100%); color: white; padding: 40px 30px; text-align: center; }}
        .header h1 {{ margin: 0; font-size: 28px; font-weight: bold; }}
        .header .icon {{ font-size: 48px; margin-bottom: 15px; }}
        .content {{ padding: 40px 30px; background: white; }}
        .footer {{ background: linear-gradient(135deg, #fae8f0 0%, #fce4ec 100%); padding: 25px; text-align: center; font-size: 12px; color: #666; border-top: 3px solid #ff8fab; }}
        .button {{ display: inline-block; background: linear-gradient(135deg, #ff8fab 0%, #fc8dc7 100%); color: white; padding: 15px 40px; text-decoration: none; border-radius: 30px; margin: 25px 0; font-weight: bold; font-size: 16px; box-shadow: 0 4px 15px rgba(233, 30, 99, 0.3); transition: all 0.3s; }}
        .button:hover {{ transform: translateY(-2px); box-shadow: 0 6px 20px rgba(233, 30, 99, 0.4); }}
        .info-box {{ background: linear-gradient(135deg, #fff5f8 0%, #ffe0eb 100%); border-left: 5px solid #ff8fab; padding: 20px; margin: 25px 0; border-radius: 8px; }}
        .features {{ background: #fef6f9; padding: 20px; border-radius: 8px; margin: 20px 0; }}
        .features ul {{ list-style: none; padding: 0; }}
        .features li {{ padding: 10px 0; padding-left: 30px; position: relative; }}
        .features li:before {{ content: '💕'; position: absolute; left: 0; font-size: 20px; }}
        .divider {{ border-top: 2px solid #ff8fab; margin: 30px 0; opacity: 0.3; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <div class='icon'>💗</div>
            <h1>Chào mừng đến với MomExchange!</h1>
            <p style='margin: 10px 0 0 0; font-size: 16px;'>Cộng đồng yêu thương cho mẹ và bé</p>
        </div>
        <div class='content'>
            <p style='font-size: 16px; color: #555;'>Xin chào <strong style='color: #e91e63; font-size: 18px;'>{fullName}</strong>,</p>
            <p style='font-size: 16px; color: #555;'>Chúc mừng! 🎉 Tài khoản của bạn đã được tạo thành công trên hệ thống <strong style='color: #e91e63;'>MomExchange</strong> - nơi kết nối yêu thương cho mẹ và bé.</p>
            
            <div class='info-box'>
                <p style='margin: 5px 0; font-size: 15px;'><strong>📧 Email đăng nhập:</strong> <span style='color: #e91e63; font-weight: bold;'>{username}</span></p>
                {passwordInfo}
            </div>

            <div class='features'>
                <p style='margin: 0 0 15px 0; font-size: 17px; font-weight: bold; color: #e91e63;'>✨ Khám phá MomExchange - Nơi mẹ và bé được yêu thương:</p>
                <ul>
                    <li><strong>Chia sẻ kinh nghiệm nuôi dạy con</strong> - Học hỏi từ những bà mẹ experienced</li>
                    <li><strong>Trao đổi đồ dùng cho bé</strong> - Tiết kiệm và bảo vệ môi trường</li>
                    <li><strong>Kết nối với cộng đồng mẹ bỉm</strong> - Cùng nhau vượt qua những khó khăn</li>
                    <li><strong>Hiến tặng và nhận sữa mẹ</strong> - Sẻ chia tình thương</li>
                </ul>
            </div>

            <div style='text-align: center; padding: 20px 0;'>
                <a href='http://localhost:53251/' class='button'>🚀 Bắt đầu ngay</a>
            </div>

            <div class='divider'></div>

            <p style='font-size: 15px; color: #666;'>💌 Nếu bạn có bất kỳ câu hỏi nào hoặc cần hỗ trợ, đừng ngại liên hệ với chúng tôi qua email: <a href='mailto:dttthao.5354@gmail.com' style='color: #e91e63; font-weight: bold;'>dttthao.5354@gmail.com</a></p>
            
            <p style='margin-top: 30px; font-size: 15px;'>Trân trọng,<br><strong style='color: #e91e63; font-size: 17px;'>💕 Đội ngũ MomExchange</strong></p>
        </div>
        <div class='footer'>
            <p style='margin: 5px 0;'><strong>&copy; {DateTime.Now.Year} MomExchange.</strong> Tất cả quyền được bảo lưu.</p>
            <p style='margin: 5px 0;'>💗 Email này được gửi tự động, vui lòng không trả lời trực tiếp.</p>
            <p style='margin: 10px 0 0 0; font-size: 11px; color: #999;'>From: <a href='mailto:dttthao.5354@gmail.com' style='color: #e91e63;'>dttthao.5354@gmail.com</a></p>
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

