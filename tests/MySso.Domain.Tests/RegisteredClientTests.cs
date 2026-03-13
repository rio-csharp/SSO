using MySso.Domain.Entities;
using MySso.Domain.Enums;

namespace MySso.Domain.Tests;

public sealed class RegisteredClientTests
{
    [Fact]
    public void Create_Requires_Pkce()
    {
        var now = new DateTimeOffset(2026, 3, 13, 0, 0, 0, TimeSpan.Zero);

        Assert.Throws<ArgumentException>(() => RegisteredClient.Create(
            Guid.NewGuid(),
            "sample-client",
            "Sample Client",
            ClientType.Public,
            false,
            true,
            false,
            new[] { "https://client.example.com/signin-oidc" },
            new[] { "openid", "profile" },
            now));
    }

    [Fact]
    public void Create_Rejects_NonAbsolute_RedirectUri()
    {
        var now = new DateTimeOffset(2026, 3, 13, 0, 0, 0, TimeSpan.Zero);

        Assert.Throws<ArgumentException>(() => RegisteredClient.Create(
            Guid.NewGuid(),
            "sample-client",
            "Sample Client",
            ClientType.Public,
            true,
            true,
            false,
            new[] { "/signin-oidc" },
            new[] { "openid" },
            now));
    }
}