using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MySso.Infrastructure.Persistence.Configurations;

internal static class IdentitySchemaConfiguration
{
    public static void ConfigureIdentitySchema(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<IdentityUserClaim<Guid>>().ToTable("auth_user_claims");
        modelBuilder.Entity<IdentityUserLogin<Guid>>().ToTable("auth_user_logins");
        modelBuilder.Entity<IdentityUserToken<Guid>>().ToTable("auth_user_tokens");
        modelBuilder.Entity<IdentityUserRole<Guid>>().ToTable("auth_user_roles");
        modelBuilder.Entity<IdentityRoleClaim<Guid>>().ToTable("auth_role_claims");
    }
}