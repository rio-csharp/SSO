using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MySso.Domain.Entities;
using MySso.Domain.ValueObjects;

namespace MySso.Infrastructure.Persistence.Configurations;

internal sealed class IdentityUserConfiguration : IEntityTypeConfiguration<IdentityUser>
{
    public void Configure(EntityTypeBuilder<IdentityUser> builder)
    {
        builder.ToTable("identity_users");

        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.Id).HasColumnName("id");
        builder.Property(entity => entity.Email)
            .HasColumnName("email")
            .HasMaxLength(320)
            .HasConversion(value => value.Value, value => new EmailAddress(value));
        builder.Property(entity => entity.GivenName)
            .HasColumnName("given_name")
            .HasMaxLength(100)
            .HasConversion(value => value.Value, value => new PersonName(value));
        builder.Property(entity => entity.FamilyName)
            .HasColumnName("family_name")
            .HasMaxLength(100)
            .HasConversion(value => value.Value, value => new PersonName(value));
        builder.Property(entity => entity.IsActive).HasColumnName("is_active");
        builder.Property(entity => entity.LastSignedInAtUtc).HasColumnName("last_signed_in_at_utc");
        builder.Property(entity => entity.LastSignedInIpAddress).HasColumnName("last_signed_in_ip_address").HasMaxLength(64);
        builder.Property<DateTimeOffset>("CreatedAtUtc").HasColumnName("created_at_utc");
        builder.Property<DateTimeOffset>("UpdatedAtUtc").HasColumnName("updated_at_utc");

        builder.HasIndex(entity => entity.Email).IsUnique();
    }
}