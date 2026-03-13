using Microsoft.EntityFrameworkCore;
using MySso.Application.Common.Interfaces;
using MySso.Domain.Entities;
using MySso.Infrastructure.Persistence.Configurations;

namespace MySso.Infrastructure.Persistence;

public sealed class MySsoDbContext : DbContext, IUnitOfWork
{
    public MySsoDbContext(DbContextOptions<MySsoDbContext> options)
        : base(options)
    {
    }

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    public DbSet<IdentityUser> IdentityUsers => Set<IdentityUser>();

    public DbSet<RegisteredClient> RegisteredClients => Set<RegisteredClient>();

    public DbSet<Role> Roles => Set<Role>();

    public DbSet<UserSession> UserSessions => Set<UserSession>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new AuditLogConfiguration());
        modelBuilder.ApplyConfiguration(new IdentityUserConfiguration());
        modelBuilder.ApplyConfiguration(new RegisteredClientConfiguration());
        modelBuilder.ApplyConfiguration(new RoleConfiguration());
        modelBuilder.ApplyConfiguration(new UserSessionConfiguration());
    }

    async Task IUnitOfWork.SaveChangesAsync(CancellationToken cancellationToken)
    {
        _ = await SaveChangesAsync(cancellationToken);
    }
}