using Identity.Domain.Common;
using Identity.Domain.User.Exceptions;

namespace Identity.Domain.User.Entities;

public class EmailVerificationToken : BaseEntity
{
    public Guid UserId { get; private set; }
    public string TokenHash { get; private set; } = null!;
    public DateTime ExpiresAtUtc { get; private set; }
    public DateTime? UsedAtUtc { get; private set; }

    protected EmailVerificationToken()
    {
    }

    private EmailVerificationToken(Guid userId, string tokenHash, TimeSpan lifetime)
    {
        if (userId == Guid.Empty)
            throw new InvalidVerificationTokenException();

        if (string.IsNullOrWhiteSpace(tokenHash))
            throw new InvalidVerificationTokenException();

        UserId = userId;
        TokenHash = tokenHash;
        ExpiresAtUtc = DateTime.UtcNow.Add(lifetime);
    }

    public static EmailVerificationToken Create(Guid userId, string tokenHash, TimeSpan lifetime)
        => new(userId, tokenHash, lifetime);

    public bool IsValid(DateTime utcNow)
        => UsedAtUtc is null && utcNow <= ExpiresAtUtc;

    public void MarkUsed(DateTime utcNow)
    {
        if (!IsValid(utcNow))
            throw new InvalidVerificationTokenException();

        UsedAtUtc = utcNow;
        SetUpdatedAt();
    }

    public void Invalidate(DateTime utcNow)
    {
        if (UsedAtUtc is not null)
            return;

        UsedAtUtc = utcNow;
        SetUpdatedAt();
    }
}
