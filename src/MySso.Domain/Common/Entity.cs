namespace MySso.Domain.Common;

public abstract class Entity
{
    protected Entity(Guid id)
    {
        Id = Guard.AgainstEmpty(id, nameof(id));
    }

    public Guid Id { get; }
}