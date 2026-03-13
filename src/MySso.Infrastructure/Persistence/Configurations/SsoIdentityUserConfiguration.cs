using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MySso.Infrastructure.Identity;

namespace MySso.Infrastructure.Persistence.Configurations;

internal sealed class SsoIdentityUserConfiguration : IEntityTypeConfiguration<SsoIdentityUser>
{
    public void Configure(EntityTypeBuilder<SsoIdentityUser> builder)
    {
        builder.ToTable("auth_users");

        builder.Property(entity => entity.DomainUserId).HasColumnName("domain_user_id");
        builder.Property(entity => entity.GivenName).HasColumnName("given_name").HasMaxLength(100);
        builder.Property(entity => entity.FamilyName).HasColumnName("family_name").HasMaxLength(100);
        builder.Property(entity => entity.IsActive).HasColumnName("is_active");

        builder.HasIndex(entity => entity.DomainUserId);
    }
}