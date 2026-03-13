using MySso.Domain.Common;
using MySso.Domain.Enums;

namespace MySso.Domain.Entities;

public sealed class UserSession : AuditableEntity
{
    private UserSession(
        Guid id,
        Guid userId,
        string subject,
        string? clientId,
        DateTimeOffset createdAtUtc,
        DateTimeOffset expiresAtUtc)
        : base(id, createdAtUtc)
    {
        UserId = Guard.AgainstEmpty(userId, nameof(userId));
        Subject = Guard.AgainstNullOrWhiteSpace(subject, nameof(subject));
        ClientId = string.IsNullOrWhiteSpace(clientId) ? null : clientId.Trim();

        if (expiresAtUtc <= createdAtUtc)
        {
            throw new ArgumentOutOfRangeException(nameof(expiresAtUtc), "Session expiry must be later than creation time.");
        }

        ExpiresAtUtc = expiresAtUtc;
    }

    public Guid UserId { get; }

    public string Subject { get; }

    public string? ClientId { get; }

    public DateTimeOffset ExpiresAtUtc { get; private set; }

    public bool IsRevoked { get; private set; }

    public DateTimeOffset? RevokedAtUtc { get; private set; }

    public string? RevokedBy { get; private set; }

    public SessionRevocationReason? RevocationReason { get; private set; }

    public static UserSession Start(
        Guid id,
        Guid userId,
        string subject,
        string? clientId,
        DateTimeOffset createdAtUtc,
        DateTimeOffset expiresAtUtc)
        => new(id, userId, subject, clientId, createdAtUtc, expiresAtUtc);

    public void Extend(DateTimeOffset expiresAtUtc, DateTimeOffset updatedAtUtc)
    {
        if (IsRevoked)
        {
            throw new InvalidOperationException("Revoked sessions cannot be extended.");
        }

        if (expiresAtUtc <= updatedAtUtc)
        {
            throw new ArgumentOutOfRangeException(nameof(expiresAtUtc), "Session expiry must be later than the update time.");
        }

        ExpiresAtUtc = expiresAtUtc;
        Touch(updatedAtUtc);
    }

    public void Revoke(string revokedBy, SessionRevocationReason reason, DateTimeOffset revokedAtUtc)
    {
        if (IsRevoked)
        {
            throw new InvalidOperationException("Session has already been revoked.");
        }

        RevokedBy = Guard.AgainstNullOrWhiteSpace(revokedBy, nameof(revokedBy));
        RevokedAtUtc = revokedAtUtc;
        RevocationReason = reason;
        IsRevoked = true;
        Touch(revokedAtUtc);
    }

    public bool IsActiveAt(DateTimeOffset now)
        => !IsRevoked && now < ExpiresAtUtc;
}