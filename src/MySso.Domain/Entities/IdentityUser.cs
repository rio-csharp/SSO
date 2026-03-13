using MySso.Domain.Common;
using MySso.Domain.ValueObjects;

namespace MySso.Domain.Entities;

public sealed class IdentityUser : AuditableEntity
{
    private IdentityUser()
    {
    }

    private IdentityUser(Guid id, EmailAddress email, PersonName givenName, PersonName familyName, DateTimeOffset createdAtUtc)
        : base(id, createdAtUtc)
    {
        Email = email;
        GivenName = givenName;
        FamilyName = familyName;
        IsActive = true;
    }

    public EmailAddress Email { get; private set; }

    public PersonName GivenName { get; private set; }

    public PersonName FamilyName { get; private set; }

    public bool IsActive { get; private set; }

    public DateTimeOffset? LastSignedInAtUtc { get; private set; }

    public string? LastSignedInIpAddress { get; private set; }

    public string DisplayName => $"{GivenName} {FamilyName}";

    public static IdentityUser Create(Guid id, EmailAddress email, PersonName givenName, PersonName familyName, DateTimeOffset createdAtUtc)
        => new(id, email, givenName, familyName, createdAtUtc);

    public void Rename(PersonName givenName, PersonName familyName, DateTimeOffset updatedAtUtc)
    {
        GivenName = givenName;
        FamilyName = familyName;
        Touch(updatedAtUtc);
    }

    public void ChangeEmail(EmailAddress email, DateTimeOffset updatedAtUtc)
    {
        Email = email;
        Touch(updatedAtUtc);
    }

    public void Activate(DateTimeOffset updatedAtUtc)
    {
        IsActive = true;
        Touch(updatedAtUtc);
    }

    public void Deactivate(DateTimeOffset updatedAtUtc)
    {
        IsActive = false;
        Touch(updatedAtUtc);
    }

    public void RecordSuccessfulSignIn(string ipAddress, DateTimeOffset occurredAtUtc)
    {
        LastSignedInIpAddress = Guard.AgainstNullOrWhiteSpace(ipAddress, nameof(ipAddress));
        LastSignedInAtUtc = occurredAtUtc;
        Touch(occurredAtUtc);
    }
}