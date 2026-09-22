using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
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
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<EmailSenderService> _logger;

        public EmailSenderService(IConfiguration config, IHttpClientFactory httpClientFactory, ILogger<EmailSenderService> logger)
        {
            _config = config;
            _httpClientFactory = httpClientFactory;
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
            var token = _config["Mailtrap:ApiToken"];
            var senderEmail = _config["Mailtrap:SenderEmail"] ?? "hello@demomailtrap.co";
            var senderName = _config["Mailtrap:SenderName"] ?? "Wainfo HR Team";
            var endpoint = _config["Mailtrap:ApiEndpoint"] ?? "https://send.api.mailtrap.io/api/send";

            string htmlContent = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 24px; border: 1px solid #e2e8f0; border-radius: 10px; background-color: #ffffff;'>
                    <div style='text-align: center; margin-bottom: 24px;'>
                        <h2 style='color: #2563eb; margin: 0;'>Interview Invitation</h2>
                        <p style='color: #64748b; font-size: 14px; margin-top: 4px;'>Role: <strong>{jobTitle}</strong></p>
                    </div>
                    
                    <p style='font-size: 15px; color: #1e293b;'>Dear <strong>{candidateName}</strong>,</p>
                    <p style='font-size: 14px; color: #475569; line-height: 1.6;'>
                        We have reviewed your application and would like to invite you for the next round of discussion with our team.
                    </p>

                    <div style='background-color: #f8fafc; border-left: 4px solid #2563eb; padding: 16px; margin: 24px 0; border-radius: 4px;'>
                        <p style='margin: 6px 0; color: #334155; font-size: 14px;'><strong>Round:</strong> {round}</p>
                        <p style='margin: 6px 0; color: #334155; font-size: 14px;'><strong>Date:</strong> {dateStr}</p>
                        <p style='margin: 6px 0; color: #334155; font-size: 14px;'><strong>Time:</strong> {timeStr}</p>
                        <p style='margin: 6px 0; color: #334155; font-size: 14px;'><strong>Interviewer:</strong> {interviewer}</p>
                        <p style='margin: 6px 0; color: #334155; font-size: 14px;'><strong>Mode:</strong> Online Video Call</p>
                    </div>

                    <div style='text-align: center; margin: 30px 0;'>
                        <a href='{meetLink}' style='background-color: #2563eb; color: #ffffff; padding: 14px 28px; text-decoration: none; border-radius: 6px; font-weight: bold; font-size: 15px; display: inline-block; box-shadow: 0 4px 6px -1px rgba(37, 99, 235, 0.2);'>Join Google Meet</a>
                    </div>

                    <p style='color: #64748b; font-size: 12px; line-height: 1.5;'>
                        Direct Link: <a href='{meetLink}' style='color: #2563eb;'>{meetLink}</a><br/>
                        Please ensure a stable internet connection and a working webcam/microphone.
                    </p>

                    <hr style='border: none; border-top: 1px solid #f1f5f9; margin: 24px 0;'/>
                    <p style='color: #94a3b8; font-size: 11px; text-align: center; margin: 0;'>
                        Wainfo Pvt Ltd &bull; Recruitment Management System &bull; Confidential
                    </p>
                </div>";

            var payload = new
            {
                from = new { email = senderEmail, name = senderName },
                to = new[] { new { email = toEmail, name = candidateName } },
                subject = $"Interview Scheduled: {jobTitle} ({round}) - Wainfo Pvt Ltd",
                html = htmlContent,
                category = "Recruitment Interview Invite"
            };

            try
            {
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var jsonPayload = JsonSerializer.Serialize(payload);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(endpoint, content);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Mailtrap email sent successfully to {Email}", toEmail);
                    return (true, $"Meeting invite sent to {toEmail} via Mailtrap.");
                }
                else
                {
                    _logger.LogError("Mailtrap API error: {StatusCode} - {Body}", response.StatusCode, responseBody);
                    return (false, $"Mailtrap error: {response.StatusCode}. Details: {responseBody}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to dispatch email to {Email}", toEmail);
                return (false, $"Exception: {ex.Message}");
            }
        }
    }
}
