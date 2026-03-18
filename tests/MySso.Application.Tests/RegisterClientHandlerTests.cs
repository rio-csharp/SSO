using MySso.Application.Common.Exceptions;
using MySso.Application.Features.Clients;
using MySso.Domain.Enums;

namespace MySso.Application.Tests;

public sealed class RegisterClientHandlerTests
{
    [Fact]
    public async Task HandleAsync_Registers_Client_And_Provisions_OpenIddict_Application()
    {
        var clients = new FakeClientRepository();
        var provisioning = new FakeClientProvisioningService();
        var audits = new FakeAuditLogRepository();
        var unitOfWork = new FakeUnitOfWork();
        var handler = new RegisterClientHandler(
            clients,
            provisioning,
            audits,
            FakeCurrentUserContext.Administrator(),
            new FakeDateTimeProvider(new DateTimeOffset(2026, 3, 18, 8, 0, 0, TimeSpan.Zero)),
            unitOfWork);

        var result = await handler.HandleAsync(
            new RegisterClientCommand(
                "partner-portal",
                "Partner Portal",
                ClientType.Confidential,
                true,
                true,
                false,
                new[] { "https://partner.example.com/signin-oidc" },
                new[] { "openid", "profile", "api" },
                "super-secret-value",
                "https://partner.example.com/signout-callback-oidc"));

        Assert.True(result.Succeeded);
        Assert.Single(clients.Clients);
        Assert.Single(audits.AuditLogs);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
        Assert.Equal(1, provisioning.ProvisionCallCount);
        Assert.Equal("super-secret-value", provisioning.LastClientSecret);
        Assert.Equal("https://partner.example.com/signout-callback-oidc", provisioning.LastPostLogoutRedirectUri);
    }

    [Fact]
    public async Task HandleAsync_Rejects_Duplicate_ClientId_When_OpenIddict_Store_Already_Contains_It()
    {
        var handler = new RegisterClientHandler(
            new FakeClientRepository(),
            new FakeClientProvisioningService { ExistsResult = true },
            new FakeAuditLogRepository(),
            FakeCurrentUserContext.Administrator(),
            new FakeDateTimeProvider(new DateTimeOffset(2026, 3, 18, 8, 0, 0, TimeSpan.Zero)),
            new FakeUnitOfWork());

        var result = await handler.HandleAsync(
            new RegisterClientCommand(
                "sample-client-web",
                "Duplicate",
                ClientType.Confidential,
                true,
                true,
                false,
                new[] { "https://localhost:7041/signin-oidc" },
                new[] { "openid" },
                "sample-client-secret",
                "https://localhost:7041/signout-callback-oidc"));

        Assert.False(result.Succeeded);
        Assert.Equal("clients.duplicate_client_id", result.ErrorCode);
    }

    [Fact]
    public async Task HandleAsync_Rejects_NonAdministrator()
    {
        var handler = new RegisterClientHandler(
            new FakeClientRepository(),
            new FakeClientProvisioningService(),
            new FakeAuditLogRepository(),
            FakeCurrentUserContext.AuthenticatedUser("user-1"),
            new FakeDateTimeProvider(new DateTimeOffset(2026, 3, 18, 8, 0, 0, TimeSpan.Zero)),
            new FakeUnitOfWork());

        await Assert.ThrowsAsync<ForbiddenAccessException>(() => handler.HandleAsync(
            new RegisterClientCommand(
                "partner-portal",
                "Partner Portal",
                ClientType.Public,
                true,
                false,
                false,
                new[] { "https://partner.example.com/signin-oidc" },
                new[] { "openid" },
                null,
                null)));
    }
}