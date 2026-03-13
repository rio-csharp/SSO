using MySso.Domain.ValueObjects;

namespace MySso.Domain.Tests;

public sealed class EmailAddressTests
{
    [Fact]
    public void Constructor_Normalizes_Address()
    {
        var email = new EmailAddress("  USER@Example.COM ");

        Assert.Equal("user@example.com", email.Value);
    }

    [Fact]
    public void Constructor_Rejects_Invalid_Address()
    {
        Assert.Throws<ArgumentException>(() => new EmailAddress("not-an-email"));
    }
}