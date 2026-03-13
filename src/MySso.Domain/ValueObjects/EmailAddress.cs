using System.Net.Mail;
using MySso.Domain.Common;

namespace MySso.Domain.ValueObjects;

public readonly record struct EmailAddress
{
    public EmailAddress(string value)
    {
        var normalized = Guard.AgainstNullOrWhiteSpace(value, nameof(value)).ToLowerInvariant();

        if (!MailAddress.TryCreate(normalized, out var parsed) || !string.Equals(parsed.Address, normalized, StringComparison.Ordinal))
        {
            throw new ArgumentException("Email address format is invalid.", nameof(value));
        }

        Value = normalized;
    }

    public string Value { get; }

    public override string ToString() => Value;

    public static implicit operator string(EmailAddress emailAddress) => emailAddress.Value;
}