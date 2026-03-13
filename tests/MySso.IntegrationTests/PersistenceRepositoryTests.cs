using Microsoft.EntityFrameworkCore;
using MySso.Domain.Entities;
using MySso.Domain.ValueObjects;
using MySso.Infrastructure.Persistence;
using MySso.Infrastructure.Persistence.Repositories;

namespace MySso.IntegrationTests;

public sealed class PersistenceRepositoryTests
{
    [Fact]
    public async Task EfUserRepository_Persists_And_Finds_User_By_Email()
    {
        await using var dbContext = CreateDbContext();
        var repository = new EfUserRepository(dbContext);
        var user = IdentityUser.Create(
            Guid.NewGuid(),
            new EmailAddress("jane@example.com"),
            new PersonName("Jane"),
            new PersonName("Doe"),
            new DateTimeOffset(2026, 3, 13, 8, 0, 0, TimeSpan.Zero));

        await repository.AddAsync(user, CancellationToken.None);
        await dbContext.SaveChangesAsync();

        var exists = await repository.ExistsByEmailAsync(new EmailAddress("jane@example.com"), CancellationToken.None);

        Assert.True(exists);
    }

    [Fact]
    public async Task EfAuditLogRepository_Persists_Metadata()
    {
        await using var dbContext = CreateDbContext();
        var repository = new EfAuditLogRepository(dbContext);
        var auditLog = AuditLog.Create(
            Guid.NewGuid(),
            "admin-1",
            Domain.Enums.AuditActionType.UserCreated,
            "IdentityUser",
            Guid.NewGuid().ToString(),
            true,
            new DateTimeOffset(2026, 3, 13, 8, 0, 0, TimeSpan.Zero),
            "127.0.0.1",
            "Created user.",
            new Dictionary<string, string> { ["email"] = "jane@example.com" });

        await repository.AddAsync(auditLog, CancellationToken.None);
        await dbContext.SaveChangesAsync();

        var persisted = await dbContext.AuditLogs.SingleAsync();

        Assert.Equal("jane@example.com", persisted.Metadata["email"]);
    }

    private static MySsoDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<MySsoDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new MySsoDbContext(options);
    }
}