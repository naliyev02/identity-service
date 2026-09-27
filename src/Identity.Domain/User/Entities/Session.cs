using Identity.Domain.Common;

namespace Identity.Domain.User.Entities;

public class Session : BaseEntity
{
    public Guid UserId { get; private set; }
    public string TokenHash { get; private set; } = null!;
    public DateTime ExpiresAtUtc { get; private set; }
    public DateTime? RevokedAtUtc { get; private set; }
    public Guid? ReplacedBySessionId { get; private set; }

    protected Session()
    {
    }

    private Session(Guid userId, string tokenHash, TimeSpan lifetime)
    {
        UserId = userId;
        TokenHash = tokenHash;
        ExpiresAtUtc = DateTime.UtcNow.Add(lifetime);
    }

    public static Session Create(Guid userId, string tokenHash, TimeSpan lifetime)
        => new(userId, tokenHash, lifetime);

    public bool IsActive(DateTime utcNow)
        => RevokedAtUtc is null && utcNow < ExpiresAtUtc;

    public bool WasRotated => ReplacedBySessionId is not null;

    public void ReplaceWith(Guid newSessionId, DateTime utcNow)
    {
        ReplacedBySessionId = newSessionId;
        Revoke(utcNow);
    }

    public void Revoke(DateTime utcNow)
    {
        if (RevokedAtUtc is not null)
            return;

        RevokedAtUtc = utcNow;
        SetUpdatedAt();
    }
}
