using System.Net;
using System.Net.Mail;

namespace mvcDapper3.AppCodes.AppService
{
    public class EmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task SendActivationEmailAsync(string recipientEmail, string activationLink)
        {
            var gmailConfig = _config.GetSection("Gmail");
            
            using var smtpClient = new SmtpClient(gmailConfig["HostUrl"])
            {
                Port = int.Parse(gmailConfig["Port"]),
                Credentials = new NetworkCredential(
                    gmailConfig["UserName"],
                    gmailConfig["AppPassword"]),
                EnableSsl = bool.Parse(gmailConfig["UseSSL"])
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(
                    gmailConfig["SenderEmail"],
                    gmailConfig["SenderName"]),
                Subject = "帳號啟用通知",
                Body = $@"<h1>感謝您的註冊</h1>
                        <p>請點擊以下連結啟用您的帳號：</p>
                        <p><a href='{activationLink}'>{activationLink}</a></p>
                        <p>此連結將在2小時後失效</p>
                        <p>本信件為系統自動寄出,請勿回覆!!</p><br />",
                IsBodyHtml = true
            };
            mailMessage.To.Add(recipientEmail);

            try
            {
                await smtpClient.SendMailAsync(mailMessage);
                Log.Information($"啟用郵件已發送至 {recipientEmail}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"發送啟用郵件失敗: {ex.Message}");
                throw;
            }
        }

        public async Task SendPasswordResetEmailAsync(string recipientEmail, string subject, string body)
        {
            var gmailConfig = _config.GetSection("Gmail");

            using var smtpClient = new SmtpClient(gmailConfig["HostUrl"])
            {
                Port = int.Parse(gmailConfig["Port"]),
                Credentials = new NetworkCredential(
                    gmailConfig["UserName"],
                    gmailConfig["AppPassword"]),
                EnableSsl = bool.Parse(gmailConfig["UseSSL"])
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(
                    gmailConfig["SenderEmail"],
                    gmailConfig["SenderName"]),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            mailMessage.To.Add(recipientEmail);

            try
            {
                await smtpClient.SendMailAsync(mailMessage);
                Log.Information($"郵件已發送至 {recipientEmail}，主旨: {subject}");
            }
            catch (Exception ex)
            {
                Log.Error($"發送郵件失敗 ({subject}): {ex.Message}");
                throw;
            }
        }
    }
}
