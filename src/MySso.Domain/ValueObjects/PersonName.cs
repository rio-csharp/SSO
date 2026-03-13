using MySso.Domain.Common;

namespace MySso.Domain.ValueObjects;

public readonly record struct PersonName
{
    public PersonName(string value)
    {
        var normalized = Guard.AgainstNullOrWhiteSpace(value, nameof(value));

        if (normalized.Length > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "Name cannot exceed 100 characters.");
        }

        Value = normalized;
    }

    public string Value { get; }

    public override string ToString() => Value;

    public static implicit operator string(PersonName personName) => personName.Value;
}