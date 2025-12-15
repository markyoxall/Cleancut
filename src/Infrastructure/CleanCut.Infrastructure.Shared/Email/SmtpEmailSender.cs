using System.Net;
using System.Net.Mail;
using CleanCut.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace CleanCut.Infrastructure.Shared.Email;

public class SmtpOptions
{
    public string Host { get; set; } = "localhost";
    // Default to MailHog SMTP port for local development
    public int Port { get; set; } = 1025;
    public bool UseSsl { get; set; } = false;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromAddress { get; set; } = "no-reply@cleancut.local";
    public string FromName { get; set; } = "CleanCut";
}

public class SmtpEmailSender : IEmailSender
{
    private readonly SmtpOptions _options;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(SmtpOptions options, ILogger<SmtpEmailSender> logger)
    {
        _options = options;
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        try
        {
            using var client = new SmtpClient(_options.Host, _options.Port)
            {
                EnableSsl = _options.UseSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
            };

            if (!string.IsNullOrEmpty(_options.Username))
            {
                client.Credentials = new NetworkCredential(_options.Username, _options.Password);
            }

            var msg = new MailMessage
            {
                From = new MailAddress(_options.FromAddress, _options.FromName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };
            msg.To.Add(to);

            await client.SendMailAsync(msg, cancellationToken);
            _logger.LogInformation("Sent email to {To}", to);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send email to {To}", to);
        }
    }
}
