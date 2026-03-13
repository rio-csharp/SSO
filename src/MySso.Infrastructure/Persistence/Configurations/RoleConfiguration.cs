using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MySso.Domain.Entities;

namespace MySso.Infrastructure.Persistence.Configurations;

internal sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("roles");

        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.Id).HasColumnName("id");
        builder.Property(entity => entity.Name).HasColumnName("name").HasMaxLength(100);
        builder.Property(entity => entity.Description).HasColumnName("description").HasMaxLength(500);
        builder.Property(entity => entity.IsSystemRole).HasColumnName("is_system_role");
        builder.Property<DateTimeOffset>("CreatedAtUtc").HasColumnName("created_at_utc");
        builder.Property<DateTimeOffset>("UpdatedAtUtc").HasColumnName("updated_at_utc");

        builder.HasIndex(entity => entity.Name).IsUnique();
    }
}