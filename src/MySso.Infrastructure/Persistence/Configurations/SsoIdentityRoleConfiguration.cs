using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MySso.Infrastructure.Identity;

namespace MySso.Infrastructure.Persistence.Configurations;

internal sealed class SsoIdentityRoleConfiguration : IEntityTypeConfiguration<SsoIdentityRole>
{
    public void Configure(EntityTypeBuilder<SsoIdentityRole> builder)
    {
        builder.ToTable("auth_roles");

        builder.Property(entity => entity.DomainRoleId).HasColumnName("domain_role_id");
        builder.Property(entity => entity.Description).HasColumnName("description").HasMaxLength(500);
        builder.Property(entity => entity.IsSystemRole).HasColumnName("is_system_role");

        builder.HasIndex(entity => entity.DomainRoleId);
    }
}