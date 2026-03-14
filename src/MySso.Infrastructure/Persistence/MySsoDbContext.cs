using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MySso.Application.Common.Interfaces;
using MySso.Domain.Entities;
using MySso.Infrastructure.Identity;
using MySso.Infrastructure.Persistence.Configurations;
using OpenIddict.EntityFrameworkCore;

namespace MySso.Infrastructure.Persistence;

public sealed class MySsoDbContext : IdentityDbContext<SsoIdentityUser, SsoIdentityRole, Guid>, IUnitOfWork
{
    public MySsoDbContext(DbContextOptions<MySsoDbContext> options)
        : base(options)
    {
    }

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    public DbSet<SsoIdentityRole> IdentityRoles => Set<SsoIdentityRole>();

    public DbSet<SsoIdentityUser> IdentityAccounts => Set<SsoIdentityUser>();

    public DbSet<IdentityUser> IdentityUsers => Set<IdentityUser>();

    public DbSet<RegisteredClient> RegisteredClients => Set<RegisteredClient>();

    public DbSet<Role> DomainRoles => Set<Role>();

    public DbSet<UserSession> UserSessions => Set<UserSession>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new AuditLogConfiguration());
        modelBuilder.ApplyConfiguration(new IdentityUserConfiguration());
        modelBuilder.ApplyConfiguration(new SsoIdentityRoleConfiguration());
        modelBuilder.ApplyConfiguration(new SsoIdentityUserConfiguration());
        modelBuilder.ApplyConfiguration(new RegisteredClientConfiguration());
        modelBuilder.ApplyConfiguration(new RoleConfiguration());
        modelBuilder.ApplyConfiguration(new UserSessionConfiguration());
        modelBuilder.ConfigureIdentitySchema();
        modelBuilder.UseOpenIddict<Guid>();
    }

    async Task IUnitOfWork.SaveChangesAsync(CancellationToken cancellationToken)
    {
        _ = await SaveChangesAsync(cancellationToken);
    }
}