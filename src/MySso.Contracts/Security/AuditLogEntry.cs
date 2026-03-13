namespace MySso.Contracts.Security;

public sealed record AuditLogEntry(
    Guid Id,
    string ActorId,
    string ActionType,
    string ResourceType,
    string ResourceId,
    bool Succeeded,
    DateTimeOffset OccurredAtUtc,
    string? IpAddress,
    string Description,
    IReadOnlyDictionary<string, string> Metadata);