using MySso.Domain.Common;
using MySso.Domain.Enums;

namespace MySso.Domain.Entities;

public sealed class AuditLog : Entity
{
    private Dictionary<string, string> _metadata = new(StringComparer.OrdinalIgnoreCase);

    private AuditLog()
    {
    }

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
        _metadata = metadata is null
            ? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            : new Dictionary<string, string>(metadata, StringComparer.OrdinalIgnoreCase);
    }

    public string ActorId { get; private set; } = string.Empty;

    public AuditActionType ActionType { get; private set; }

    public string ResourceType { get; private set; } = string.Empty;

    public string ResourceId { get; private set; } = string.Empty;

    public bool Succeeded { get; private set; }

    public DateTimeOffset OccurredAtUtc { get; private set; }

    public string? IpAddress { get; private set; }

    public string Description { get; private set; } = string.Empty;

    public IReadOnlyDictionary<string, string> Metadata => _metadata;

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