using ECommerce.Application.Exceptions;
using ECommerce.Application.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;

namespace ECommerce.Infrastructure.Services;

/// <summary>
/// Real email provider backed by SMTP (e.g. Gmail, SendGrid SMTP relay, Mailgun).
/// Settings come from configuration under "Email:Smtp". When no Host is
/// configured, wire <see cref="DevelopmentEmailService"/> instead (see Program.cs).
/// </summary>
public class SmtpEmailService : IEmailService
{
    private readonly string _host;
    private readonly int _port;
    private readonly string _username;
    private readonly string _password;
    private readonly string _fromName;
    private readonly string _fromAddress;
    private readonly bool _enableSsl;
    private readonly bool _useDefaultCredentials;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IConfiguration configuration, ILogger<SmtpEmailService> logger)
    {
        var section = configuration.GetSection("Email:Smtp");
        _host = section["Host"] ?? string.Empty;
        _port = int.TryParse(section["Port"], out var port) ? port : 587;
        _username = section["Username"] ?? string.Empty;
        _password = section["Password"] ?? string.Empty;
        _fromName = section["FromName"] ?? "ECommerce";
        _fromAddress = section["FromAddress"] ?? string.Empty;
        _enableSsl = !string.Equals(section["EnableSsl"], "false", StringComparison.OrdinalIgnoreCase);
        _useDefaultCredentials = string.Equals(section["UseDefaultCredentials"], "true", StringComparison.OrdinalIgnoreCase);
        _logger = logger;
    }

    public async Task SendAsync(string to, string subject, string htmlBody)
    {
        try
        {
            using var message = new MailMessage
            {
                From = new MailAddress(_fromAddress, _fromName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };
            message.To.Add(to);

            // SmtpClient is not thread-safe, so a fresh instance is used per send.
            using var client = new SmtpClient(_host, _port)
            {
                EnableSsl = _enableSsl,
                UseDefaultCredentials = _useDefaultCredentials
            };
            if (!_useDefaultCredentials)
                client.Credentials = new NetworkCredential(_username, _password);

            await client.SendMailAsync(message);
            _logger.LogInformation("Email sent to {To} with subject {Subject}", to, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To}", to);
            throw new BusinessException($"Could not send email: {ex.Message}");
        }
    }
}
