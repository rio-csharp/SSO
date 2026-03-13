using MySso.Application.Common.Exceptions;
using MySso.Application.Common.Interfaces;
using MySso.Application.Features.IdentityUsers;

namespace MySso.Application.Tests;

public sealed class CreateLocalUserHandlerTests
{
    [Fact]
    public async Task HandleAsync_Provisions_Identity_Account()
    {
        var users = new CreateUserHandlerTests.FakeUserRepository();
        var audits = new FakeAuditLogRepository();
        var identityProvisioning = new FakeIdentityAccountProvisioningService();
        var handler = new CreateLocalUserHandler(
            users,
            audits,
            FakeCurrentUserContext.Administrator(),
            new FakeDateTimeProvider(new DateTimeOffset(2026, 3, 14, 12, 0, 0, TimeSpan.Zero)),
            identityProvisioning,
            new FakeUnitOfWork());

        var result = await handler.HandleAsync(new CreateLocalUserCommand("new.user@example.com", "New", "User", "ChangeThis123!", true));

        Assert.True(result.Succeeded);
        Assert.True(identityProvisioning.WasCalled);
        Assert.Contains("Administrator", identityProvisioning.Roles);
    }

    [Fact]
    public async Task HandleAsync_Rejects_NonAdministrator()
    {
        var handler = new CreateLocalUserHandler(
            new CreateUserHandlerTests.FakeUserRepository(),
            new FakeAuditLogRepository(),
            FakeCurrentUserContext.AuthenticatedUser("user-1"),
            new FakeDateTimeProvider(new DateTimeOffset(2026, 3, 14, 12, 0, 0, TimeSpan.Zero)),
            new FakeIdentityAccountProvisioningService(),
            new FakeUnitOfWork());

        await Assert.ThrowsAsync<ForbiddenAccessException>(() => handler.HandleAsync(new CreateLocalUserCommand("new.user@example.com", "New", "User", "ChangeThis123!", false)));
    }

    private sealed class FakeIdentityAccountProvisioningService : IIdentityAccountProvisioningService
    {
        public bool WasCalled { get; private set; }

        public IReadOnlyCollection<string> Roles { get; private set; } = Array.Empty<string>();

        public Task ProvisionLocalUserAsync(Guid domainUserId, string email, string givenName, string familyName, string password, IReadOnlyCollection<string> roles, CancellationToken cancellationToken)
        {
            WasCalled = true;
            Roles = roles;
            return Task.CompletedTask;
        }
    }
}