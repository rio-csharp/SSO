using MySso.Domain.Common;
using MySso.Domain.Enums;

namespace MySso.Domain.Entities;

public sealed class AuditLog : Entity
{
    private AuditLog(
        Guid id,
        string actorId,
        AuditActionType actionType,
        string resourceType,
        string resourceId,
        bool succeeded,
        DateTimeOffset occurredAtUtc,
        string? ipAddress,
        string description,
        IReadOnlyDictionary<string, string>? metadata)
        : base(id)
    {
        ActorId = Guard.AgainstNullOrWhiteSpace(actorId, nameof(actorId));
        ActionType = actionType;
        ResourceType = Guard.AgainstNullOrWhiteSpace(resourceType, nameof(resourceType));
        ResourceId = Guard.AgainstNullOrWhiteSpace(resourceId, nameof(resourceId));
        Succeeded = succeeded;
        OccurredAtUtc = occurredAtUtc;
        IpAddress = string.IsNullOrWhiteSpace(ipAddress) ? null : ipAddress.Trim();
        Description = Guard.AgainstNullOrWhiteSpace(description, nameof(description));
        Metadata = metadata is null
            ? new Dictionary<string, string>()
            : new Dictionary<string, string>(metadata, StringComparer.OrdinalIgnoreCase);
    }

    public string ActorId { get; }

    public AuditActionType ActionType { get; }

    public string ResourceType { get; }

    public string ResourceId { get; }

    public bool Succeeded { get; }

    public DateTimeOffset OccurredAtUtc { get; }

    public string? IpAddress { get; }

    public string Description { get; }

    public IReadOnlyDictionary<string, string> Metadata { get; }

    public static AuditLog Create(
        Guid id,
        string actorId,
        AuditActionType actionType,
        string resourceType,
        string resourceId,
        bool succeeded,
        DateTimeOffset occurredAtUtc,
        string? ipAddress,
        string description,
        IReadOnlyDictionary<string, string>? metadata = null)
        => new(id, actorId, actionType, resourceType, resourceId, succeeded, occurredAtUtc, ipAddress, description, metadata);
}