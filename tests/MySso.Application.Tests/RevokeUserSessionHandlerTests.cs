using MySso.Application.Common.Exceptions;
using MySso.Application.Common.Interfaces;
using MySso.Application.Features.UserSessions;
using MySso.Domain.Entities;
using MySso.Domain.Enums;

namespace MySso.Application.Tests;

public sealed class RevokeUserSessionHandlerTests
{
    [Fact]
    public async Task HandleAsync_Allows_Session_Owner_To_Revoke()
    {
        var createdAt = new DateTimeOffset(2026, 3, 13, 8, 0, 0, TimeSpan.Zero);
        var session = UserSession.Start(Guid.NewGuid(), Guid.NewGuid(), "user-1", "client-1", createdAt, createdAt.AddHours(1));
        var repository = new FakeUserSessionRepository(session);
        var audits = new FakeAuditLogRepository();
        var unitOfWork = new FakeUnitOfWork();
        var handler = new RevokeUserSessionHandler(
            repository,
            audits,
            FakeCurrentUserContext.AuthenticatedUser("user-1"),
            new FakeDateTimeProvider(createdAt.AddMinutes(10)),
            unitOfWork);

        var result = await handler.HandleAsync(new RevokeUserSessionCommand(session.Id, SessionRevocationReason.UserRequested));

        Assert.True(result.Succeeded);
        Assert.True(session.IsRevoked);
        Assert.Single(audits.AuditLogs);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task HandleAsync_Rejects_Unrelated_User()
    {
        var createdAt = new DateTimeOffset(2026, 3, 13, 8, 0, 0, TimeSpan.Zero);
        var session = UserSession.Start(Guid.NewGuid(), Guid.NewGuid(), "user-1", "client-1", createdAt, createdAt.AddHours(1));
        var handler = new RevokeUserSessionHandler(
            new FakeUserSessionRepository(session),
            new FakeAuditLogRepository(),
            FakeCurrentUserContext.AuthenticatedUser("user-2"),
            new FakeDateTimeProvider(createdAt.AddMinutes(10)),
            new FakeUnitOfWork());

        await Assert.ThrowsAsync<ForbiddenAccessException>(() => handler.HandleAsync(new RevokeUserSessionCommand(session.Id, SessionRevocationReason.AdministratorForced)));
    }
}