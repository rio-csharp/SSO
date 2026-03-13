namespace MySso.Contracts.Security;

public sealed record UserSessionSummary(
    Guid Id,
    Guid UserId,
    string Subject,
    string? ClientId,
    DateTimeOffset ExpiresAtUtc,
    bool IsRevoked,
    DateTimeOffset? RevokedAtUtc,
    string? RevokedBy,
    string? RevocationReason);