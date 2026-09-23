using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ERP_System.Services
{
    public interface IEmailSenderService
    {
        Task<(bool Success, string Message)> SendInterviewInvitationAsync(
            string toEmail, 
            string candidateName, 
            string jobTitle, 
            string round, 
            string dateStr, 
            string timeStr, 
            string meetLink, 
            string interviewer);
    }

    public class EmailSenderService : IEmailSenderService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailSenderService> _logger;

        public EmailSenderService(IConfiguration config, ILogger<EmailSenderService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task<(bool Success, string Message)> SendInterviewInvitationAsync(
            string toEmail, 
            string candidateName, 
            string jobTitle, 
            string round, 
            string dateStr, 
            string timeStr, 
            string meetLink, 
            string interviewer)
        {
            var host = _config["SmtpSettings:Host"] ?? "smtp.gmail.com";
            var port = _config.GetValue("SmtpSettings:Port", 587);
            var enableSsl = _config.GetValue("SmtpSettings:EnableSsl", true);
            var senderEmail = _config["SmtpSettings:SenderEmail"] ?? "affuxx00@gmail.com";
            var senderName = _config["SmtpSettings:SenderName"] ?? "Wainfo Recruitment Team";
            var username = _config["SmtpSettings:Username"] ?? senderEmail;
            var appPassword = _config["SmtpSettings:AppPassword"] ?? "jblkicpealbwskrk";

            string htmlContent = $@"
<div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 24px; border: 1px solid #e2e8f0; border-radius: 10px; background-color: #ffffff;'>
    <div style='text-align: center; margin-bottom: 24px;'>
        <h2 style='color: #2563eb; margin: 0;'>Interview Invitation</h2>
        <p style='color: #64748b; font-size: 14px; margin-top: 4px;'>Role: <strong>{jobTitle}</strong></p>
    </div>
    
    <p style='font-size: 15px; color: #1e293b;'>Dear <strong>{candidateName}</strong>,</p>
    <p style='font-size: 14px; color: #475569; line-height: 1.6;'>
        Your interview round has been scheduled with our panel at Wainfo Pvt Ltd. Please find the details below:
    </p>

    <div style='background-color: #f8fafc; border-left: 4px solid #2563eb; padding: 16px; margin: 24px 0; border-radius: 4px;'>
        <p style='margin: 6px 0; color: #334155; font-size: 14px;'><strong>Round:</strong> {round}</p>
        <p style='margin: 6px 0; color: #334155; font-size: 14px;'><strong>Date:</strong> {dateStr}</p>
        <p style='margin: 6px 0; color: #334155; font-size: 14px;'><strong>Time:</strong> {timeStr}</p>
        <p style='margin: 6px 0; color: #334155; font-size: 14px;'><strong>Interviewer:</strong> {interviewer}</p>
        <p style='margin: 6px 0; color: #334155; font-size: 14px;'><strong>Mode:</strong> Online Video Call</p>
    </div>

    <div style='text-align: center; margin: 30px 0;'>
        <a href='{meetLink}' style='background-color: #2563eb; color: #ffffff; padding: 14px 28px; text-decoration: none; border-radius: 6px; font-weight: bold; font-size: 15px; display: inline-block; box-shadow: 0 4px 6px -1px rgba(37, 99, 235, 0.2);'>Join Google Meet Call</a>
    </div>

    <p style='color: #64748b; font-size: 12px; line-height: 1.5;'>
        Direct Link: <a href='{meetLink}' style='color: #2563eb;'>{meetLink}</a>
    </p>

    <hr style='border: none; border-top: 1px solid #f1f5f9; margin: 24px 0;'/>
    <p style='color: #94a3b8; font-size: 11px; text-align: center; margin: 0;'>
        Wainfo Pvt Ltd &bull; Recruitment &amp; Talent Acquisition Team
    </p>
</div>
";

            try
            {
                using var message = new MailMessage
                {
                    From = new MailAddress(senderEmail, senderName),
                    Subject = $"Interview Invitation: {jobTitle} ({round}) - Wainfo Pvt Ltd",
                    Body = htmlContent,
                    IsBodyHtml = true
                };

                message.To.Add(new MailAddress(toEmail, candidateName));

                using var smtp = new SmtpClient(host, port)
                {
                    Credentials = new NetworkCredential(username, appPassword),
                    EnableSsl = enableSsl
                };

                await smtp.SendMailAsync(message);
                _logger.LogInformation("Real interview invite email delivered to {Recipient} via Gmail", toEmail);
                return (true, $"Interview invitation email sent successfully to {toEmail}!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email via Gmail SMTP to {Recipient}", toEmail);
                return (false, $"Gmail SMTP Error: {ex.Message}");
            }
        }
    }
}
