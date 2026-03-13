using Microsoft.EntityFrameworkCore;
using MySso.Contracts.Pagination;
using MySso.Domain.Entities;
using MySso.Domain.Enums;
using MySso.Domain.ValueObjects;
using MySso.Infrastructure.Identity;
using MySso.Infrastructure.Persistence;
using MySso.Infrastructure.Services;

namespace MySso.IntegrationTests;

public sealed class AdministrationQueryServiceTests
{
    [Fact]
    public async Task GetUsersAsync_Returns_Filtered_Page()
    {
        await using var dbContext = CreateDbContext();
        await SeedAsync(dbContext);
        var service = new AdministrationQueryService(dbContext);

        var result = await service.GetUsersAsync(new PageRequest(1, 10), "alice", CancellationToken.None);

        Assert.Single(result.Items);
        Assert.Equal("alice@example.com", result.Items.Single().Email);
    }

    [Fact]
    public async Task GetCurrentUserProfileAsync_Returns_Roles()
    {
        await using var dbContext = CreateDbContext();
        var identityId = await SeedAsync(dbContext);
        var service = new AdministrationQueryService(dbContext);

        var result = await service.GetCurrentUserProfileAsync(identityId.ToString(), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Contains("Administrator", result!.Roles);
    }

    private static MySsoDbContext CreateDbContext()
    {
        DbContextOptions<MySsoDbContext> options = new DbContextOptionsBuilder<MySsoDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new MySsoDbContext(options);
    }

    private static async Task<Guid> SeedAsync(MySsoDbContext dbContext)
    {
        var createdAt = new DateTimeOffset(2026, 3, 14, 8, 0, 0, TimeSpan.Zero);
        var user = IdentityUser.Create(Guid.NewGuid(), new EmailAddress("alice@example.com"), new PersonName("Alice"), new PersonName("Admin"), createdAt);
        var role = Role.Create(Guid.NewGuid(), "Administrator", "Admin role", true, createdAt);
        var client = RegisteredClient.Create(Guid.NewGuid(), "client-1", "Client One", ClientType.Confidential, true, true, false, new[] { "https://app.example.com/signin-oidc" }, new[] { "openid", "profile" }, createdAt);
        var session = UserSession.Start(Guid.NewGuid(), user.Id, "subject-1", "client-1", createdAt, createdAt.AddHours(1));
        var auditLog = AuditLog.Create(Guid.NewGuid(), "admin-1", AuditActionType.UserCreated, nameof(IdentityUser), user.Id.ToString(), true, createdAt, "127.0.0.1", "Created Alice.", new Dictionary<string, string>());
        var identityRole = new SsoIdentityRole { Id = Guid.NewGuid(), Name = "Administrator", NormalizedName = "ADMINISTRATOR", DomainRoleId = role.Id, Description = "Admin role", IsSystemRole = true };
        var identityUser = new SsoIdentityUser { Id = Guid.NewGuid(), UserName = "alice@example.com", NormalizedUserName = "ALICE@EXAMPLE.COM", Email = "alice@example.com", NormalizedEmail = "ALICE@EXAMPLE.COM", EmailConfirmed = true, GivenName = "Alice", FamilyName = "Admin", DomainUserId = user.Id, IsActive = true };

        await dbContext.IdentityUsers.AddAsync(user);
        await dbContext.DomainRoles.AddAsync(role);
        await dbContext.RegisteredClients.AddAsync(client);
        await dbContext.UserSessions.AddAsync(session);
        await dbContext.AuditLogs.AddAsync(auditLog);
        await dbContext.IdentityAccounts.AddAsync(identityUser);
        await dbContext.IdentityRoles.AddAsync(identityRole);
        await dbContext.UserRoles.AddAsync(new Microsoft.AspNetCore.Identity.IdentityUserRole<Guid> { UserId = identityUser.Id, RoleId = identityRole.Id });
        await dbContext.SaveChangesAsync();

        return identityUser.Id;
    }
}