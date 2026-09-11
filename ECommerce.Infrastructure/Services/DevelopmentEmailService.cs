using ECommerce.Application.Services;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Services;

/// <summary>
/// Development email provider. Does not send real mail; it logs the message so
/// the verification / reset links are visible in the console. Swap with a real
/// SMTP implementation (e.g. SmtpClient or SendGrid) in production via config.
/// </summary>
public class DevelopmentEmailService : IEmailService
{
    private readonly ILogger<DevelopmentEmailService> _logger;

    public DevelopmentEmailService(ILogger<DevelopmentEmailService> logger)
        => _logger = logger;

    public Task SendAsync(string to, string subject, string htmlBody)
    {
        _logger.LogInformation("\n[DEV-EMAIL] To: {To}\nSubject: {Subject}\nBody:\n{Body}\n",
            to, subject, htmlBody);
        return Task.CompletedTask;
    }
}