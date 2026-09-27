using Identity.Application.Abstractions.Messaging;
using Microsoft.Extensions.Logging;

namespace Identity.Infrastructure.Email;

public sealed class LoggingEmailSender : IEmailSender
{
    private readonly ILogger<LoggingEmailSender> _logger;

    public LoggingEmailSender(ILogger<LoggingEmailSender> logger)
    {
        _logger = logger;
    }

    public Task SendAsync(
        string to,
        string subject,
        string htmlBody,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Email (dev log) To={To} Subject={Subject} Body={Body}",
            to,
            subject,
            htmlBody);
        return Task.CompletedTask;
    }
}
