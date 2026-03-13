using MySso.Domain.Common;

namespace MySso.Domain.Entities;

public sealed class Role : AuditableEntity
{
    private Role(Guid id, string name, string description, bool isSystemRole, DateTimeOffset createdAtUtc)
        : base(id, createdAtUtc)
    {
        Name = NormalizeName(name);
        Description = NormalizeDescription(description);
        IsSystemRole = isSystemRole;
    }

    public string Name { get; private set; }

    public string Description { get; private set; }

    public bool IsSystemRole { get; }

    public static Role Create(Guid id, string name, string description, bool isSystemRole, DateTimeOffset createdAtUtc)
        => new(id, name, description, isSystemRole, createdAtUtc);

    public void Rename(string name, DateTimeOffset updatedAtUtc)
    {
        Name = NormalizeName(name);
        Touch(updatedAtUtc);
    }

    public void UpdateDescription(string description, DateTimeOffset updatedAtUtc)
    {
        Description = NormalizeDescription(description);
        Touch(updatedAtUtc);
    }

    private static string NormalizeName(string name)
    {
        var normalized = Guard.AgainstNullOrWhiteSpace(name, nameof(name));

        if (normalized.Length > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(name), "Role name cannot exceed 100 characters.");
        }

        return normalized;
    }

    private static string NormalizeDescription(string description)
    {
        var normalized = Guard.AgainstNullOrWhiteSpace(description, nameof(description));

        if (normalized.Length > 500)
        {
            throw new ArgumentOutOfRangeException(nameof(description), "Role description cannot exceed 500 characters.");
        }

        return normalized;
    }
}