using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MySso.Domain.Entities;
using MySso.Domain.ValueObjects;
using MySso.Infrastructure.Persistence;
using MySso.Infrastructure.Persistence.Repositories;
using MySso.Infrastructure.Services;
using MySso.Infrastructure.Options;

namespace MySso.IntegrationTests;

public sealed class SessionLifecycleServiceTests
{
    [Fact]
    public async Task StartInteractiveSessionAsync_Persists_Session_And_Updates_LastSignIn()
    {
        await using var dbContext = CreateDbContext();
        var createdAt = new DateTimeOffset(2026, 3, 14, 12, 0, 0, TimeSpan.Zero);
        var user = IdentityUser.Create(Guid.NewGuid(), new EmailAddress("jane@example.com"), new PersonName("Jane"), new PersonName("Doe"), createdAt);
        await dbContext.IdentityUsers.AddAsync(user);
        await dbContext.SaveChangesAsync();

        var now = createdAt.AddMinutes(30);
        var service = CreateService(dbContext, now);

        var sessionId = await service.StartInteractiveSessionAsync(user.Id, user.Id, "subject-123", "sample-client-web", "127.0.0.1", CancellationToken.None);

        var session = await dbContext.UserSessions.SingleAsync(item => item.Id == sessionId);
        var refreshedUser = await dbContext.IdentityUsers.SingleAsync(item => item.Id == user.Id);
        var audit = await dbContext.AuditLogs.SingleAsync(item => item.ResourceId == sessionId.ToString());

        Assert.Equal("subject-123", session.Subject);
        Assert.Equal("sample-client-web", session.ClientId);
        Assert.True(session.IsActiveAt(now));
        Assert.Equal(now, refreshedUser.LastSignedInAtUtc);
        Assert.Equal("127.0.0.1", refreshedUser.LastSignedInIpAddress);
        Assert.Equal(Domain.Enums.AuditActionType.SessionStarted, audit.ActionType);
    }

    [Fact]
    public async Task IsSessionActiveAsync_Returns_False_For_Revoked_Session()
    {
        await using var dbContext = CreateDbContext();
        var createdAt = new DateTimeOffset(2026, 3, 14, 12, 0, 0, TimeSpan.Zero);
        var session = UserSession.Start(Guid.NewGuid(), Guid.NewGuid(), "subject-123", "sample-client-web", createdAt, createdAt.AddHours(1));
        session.Revoke("admin-1", Domain.Enums.SessionRevocationReason.AdministratorForced, createdAt.AddMinutes(10));
        await dbContext.UserSessions.AddAsync(session);
        await dbContext.SaveChangesAsync();

        var service = CreateService(dbContext, createdAt.AddMinutes(20));

        var isActive = await service.IsSessionActiveAsync(session.Id, CancellationToken.None);

        Assert.False(isActive);
    }

    private static SessionLifecycleService CreateService(MySsoDbContext dbContext, DateTimeOffset now)
        => new(
            new EfUserRepository(dbContext),
            new EfUserSessionRepository(dbContext),
            new EfAuditLogRepository(dbContext),
            new SystemDateTimeProviderStub(now),
            dbContext,
            Options.Create(new MySsoHostOptions()));

    private static MySsoDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<MySsoDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new MySsoDbContext(options);
    }

    private sealed class SystemDateTimeProviderStub : MySso.Application.Common.Interfaces.IDateTimeProvider
    {
        public SystemDateTimeProviderStub(DateTimeOffset utcNow)
        {
            UtcNow = utcNow;
        }

        public DateTimeOffset UtcNow { get; }
    }
}