using Identity.Application.Abstractions.Messaging;
using Identity.Application.Abstractions.Persistence;
using Identity.Application.Abstractions.Security;
using Identity.Application.Options;
using Identity.Domain.User.Entities;
using Microsoft.Extensions.Options;

namespace Identity.Application.Services;

public sealed class EmailVerificationIssuer
{
    public static readonly TimeSpan TokenLifetime = TimeSpan.FromHours(24);

    private readonly IEmailVerificationTokenRepository _tokens;
    private readonly IVerificationTokenGenerator _tokenGenerator;
    private readonly ITokenHasher _tokenHasher;
    private readonly IEmailSender _emailSender;
    private readonly AppOptions _appOptions;

    public EmailVerificationIssuer(
        IEmailVerificationTokenRepository tokens,
        IVerificationTokenGenerator tokenGenerator,
        ITokenHasher tokenHasher,
        IEmailSender emailSender,
        IOptions<AppOptions> appOptions)
    {
        _tokens = tokens;
        _tokenGenerator = tokenGenerator;
        _tokenHasher = tokenHasher;
        _emailSender = emailSender;
        _appOptions = appOptions.Value;
    }

    public async Task<string> CreateTokenAsync(User user, CancellationToken cancellationToken = default)
    {
        var utcNow = DateTime.UtcNow;
        await _tokens.InvalidateActiveForUserAsync(user.Id, utcNow, cancellationToken);

        var rawToken = _tokenGenerator.Generate();
        var tokenHash = _tokenHasher.Hash(rawToken);
        var verificationToken = EmailVerificationToken.Create(user.Id, tokenHash, TokenLifetime);
        await _tokens.AddAsync(verificationToken, cancellationToken);
        return rawToken;
    }

    public Task SendAsync(User user, string rawToken, CancellationToken cancellationToken = default)
    {
        var baseUrl = _appOptions.PublicBaseUrl.TrimEnd('/');
        var link = $"{baseUrl}/verify-email?token={Uri.EscapeDataString(rawToken)}";
        var subject = "Email ünvanınızı təsdiqləyin";
        var body =
            $"<p>Salam {user.Name.FirstName},</p>" +
            "<p>Hesabınızı təsdiqləmək üçün düyməyə basın. Link 24 saat keçərlidir.</p>" +
            $"<p><a href=\"{link}\">Emaili təsdiqlə</a></p>";

        return _emailSender.SendAsync(user.Email.Value, subject, body, cancellationToken);
    }
}
