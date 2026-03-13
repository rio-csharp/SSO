using Microsoft.EntityFrameworkCore;
using MySso.Contracts.Pagination;
using MySso.Domain.Entities;
using MySso.Domain.ValueObjects;
using MySso.Infrastructure.Persistence;
using MySso.Infrastructure.Services;

namespace MySso.IntegrationTests;

public sealed class SessionsQueryTests
{
    [Fact]
    public async Task GetSessionsForSubjectAsync_Filters_By_Subject()
    {
        await using var dbContext = CreateDbContext();
        var createdAt = new DateTimeOffset(2026, 3, 14, 10, 0, 0, TimeSpan.Zero);
        var user = IdentityUser.Create(Guid.NewGuid(), new EmailAddress("sam@example.com"), new PersonName("Sam"), new PersonName("Session"), createdAt);
        await dbContext.IdentityUsers.AddAsync(user);
        await dbContext.UserSessions.AddAsync(UserSession.Start(Guid.NewGuid(), user.Id, "subject-a", "client-a", createdAt, createdAt.AddHours(1)));
        await dbContext.UserSessions.AddAsync(UserSession.Start(Guid.NewGuid(), user.Id, "subject-b", "client-b", createdAt, createdAt.AddHours(1)));
        await dbContext.SaveChangesAsync();

        var service = new AdministrationQueryService(dbContext);
        var result = await service.GetSessionsForSubjectAsync(new PageRequest(1, 20), "subject-a", CancellationToken.None);

        Assert.Single(result.Items);
        Assert.Equal("subject-a", result.Items.Single().Subject);
    }

    private static MySsoDbContext CreateDbContext()
    {
        DbContextOptions<MySsoDbContext> options = new DbContextOptionsBuilder<MySsoDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new MySsoDbContext(options);
    }
}