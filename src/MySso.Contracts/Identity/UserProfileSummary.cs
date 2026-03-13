namespace MySso.Contracts.Identity;

public sealed record UserProfileSummary(
    Guid IdentityAccountId,
    Guid? DomainUserId,
    string Email,
    string GivenName,
    string FamilyName,
    bool IsActive,
    IReadOnlyCollection<string> Roles,
    DateTimeOffset? LastSignedInAtUtc,
    string? LastSignedInIpAddress);