using MySso.Application.Common.Exceptions;
using MySso.Application.Common.Interfaces;
using MySso.Application.Features.IdentityUsers;
using MySso.Domain.Entities;
using MySso.Domain.ValueObjects;

namespace MySso.Application.Tests;

public sealed class CreateUserHandlerTests
{
    [Fact]
    public async Task HandleAsync_Creates_User_And_Audit_Record()
    {
        var users = new FakeUserRepository();
        var audits = new FakeAuditLogRepository();
        var unitOfWork = new FakeUnitOfWork();
        var currentUser = FakeCurrentUserContext.Administrator();
        var dateTimeProvider = new FakeDateTimeProvider(new DateTimeOffset(2026, 3, 13, 8, 0, 0, TimeSpan.Zero));
        var handler = new CreateUserHandler(users, audits, currentUser, dateTimeProvider, unitOfWork);

        var result = await handler.HandleAsync(new CreateUserCommand("alice@example.com", "Alice", "Admin"));

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Value);
        Assert.Single(users.Users);
        Assert.Single(audits.AuditLogs);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task HandleAsync_Rejects_NonAdministrator()
    {
        var handler = new CreateUserHandler(
            new FakeUserRepository(),
            new FakeAuditLogRepository(),
            FakeCurrentUserContext.AuthenticatedUser("user-1"),
            new FakeDateTimeProvider(new DateTimeOffset(2026, 3, 13, 8, 0, 0, TimeSpan.Zero)),
            new FakeUnitOfWork());

        await Assert.ThrowsAsync<ForbiddenAccessException>(() => handler.HandleAsync(new CreateUserCommand("alice@example.com", "Alice", "Admin")));
    }

    private sealed class FakeUserRepository : IUserRepository
    {
        public List<IdentityUser> Users { get; } = new();

        public Task AddAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            Users.Add(user);
            return Task.CompletedTask;
        }

        public Task<bool> ExistsByEmailAsync(EmailAddress email, CancellationToken cancellationToken)
            => Task.FromResult(Users.Any(user => user.Email == email));

        public Task<IdentityUser?> GetByIdAsync(Guid userId, CancellationToken cancellationToken)
            => Task.FromResult(Users.SingleOrDefault(user => user.Id == userId));

        public Task UpdateAsync(IdentityUser user, CancellationToken cancellationToken)
            => Task.CompletedTask;
    }
}