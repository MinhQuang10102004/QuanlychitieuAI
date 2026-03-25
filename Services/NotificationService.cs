using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace QuanLyChiTieu.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ILogger<NotificationService> _logger;
        private readonly IConfiguration _config;

        public NotificationService(ILogger<NotificationService> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            await Task.Run(() => 
            {
                try
                {
                    var smtpHost = _config["Email:SmtpHost"];

                    if (string.IsNullOrWhiteSpace(smtpHost))
                    {
                        _logger.LogWarning("[Email] SMTP not configured. Would have sent to {Email}", toEmail);
                        _logger.LogInformation("[Email] Subject: {Subject}", subject);
                        return;
                    }

                    // Actual SMTP sending logic would go here
                    // For now, we log it
                    _logger.LogInformation("[Email] Email feature ready. To: {Email}", toEmail);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[Email] Failed to send to {Email}", toEmail);
                }
            });
        }

        public async Task SendSmsAsync(string toPhone, string message)
        {
            await Task.Run(() => {
                try
                {
                    var accountSid = _config["Sms:TwilioAccountSid"];
                    var authToken = _config["Sms:TwilioAuthToken"];

                    if (string.IsNullOrWhiteSpace(accountSid) || string.IsNullOrWhiteSpace(authToken))
                    {
                        _logger.LogWarning("[SMS] Twilio not configured, logging only: to {Phone}", toPhone);
                        _logger.LogInformation("[SMS] Message: {Message}", message);
                        return;
                    }

                    // TODO: Integrate Twilio SDK (install NuGet: Twilio)
                    _logger.LogInformation("[SMS] SMS capability not yet integrated. Target: {Phone}, Message: {Message}", toPhone, message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[SMS] Failed to send to {Phone}", toPhone);
                }
            });
        }
    }
}
