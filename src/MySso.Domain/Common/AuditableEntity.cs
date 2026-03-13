namespace MySso.Domain.Common;

public abstract class AuditableEntity : Entity
{
    protected AuditableEntity(Guid id, DateTimeOffset createdAtUtc)
        : base(id)
    {
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;
    }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    protected void Touch(DateTimeOffset updatedAtUtc)
    {
        if (updatedAtUtc < CreatedAtUtc)
        {
            throw new ArgumentOutOfRangeException(nameof(updatedAtUtc), "Updated time cannot be earlier than created time.");
        }

        UpdatedAtUtc = updatedAtUtc;
    }
}