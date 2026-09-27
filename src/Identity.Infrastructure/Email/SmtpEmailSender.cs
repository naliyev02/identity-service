using Identity.Application.Abstractions.Messaging;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Identity.Infrastructure.Email;

public sealed class SmtpEmailSender : IEmailSender
{
    private readonly SmtpEmailOptions _options;

    public SmtpEmailSender(IOptions<SmtpEmailOptions> options)
    {
        _options = options.Value;
    }

    public async Task SendAsync(
        string to,
        string subject,
        string htmlBody,
        CancellationToken cancellationToken = default)
    {
        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse(_options.From));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = htmlBody };

        using var client = new SmtpClient();
        var secureSocket = _options.UseSsl
            ? SecureSocketOptions.StartTls
            : SecureSocketOptions.Auto;

        await client.ConnectAsync(_options.Host, _options.Port, secureSocket, cancellationToken);

        if (!string.IsNullOrWhiteSpace(_options.UserName))
            await client.AuthenticateAsync(_options.UserName, _options.Password, cancellationToken);

        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);
    }
}
