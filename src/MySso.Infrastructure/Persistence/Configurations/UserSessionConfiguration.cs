using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MySso.Domain.Entities;

namespace MySso.Infrastructure.Persistence.Configurations;

internal sealed class UserSessionConfiguration : IEntityTypeConfiguration<UserSession>
{
    public void Configure(EntityTypeBuilder<UserSession> builder)
    {
        builder.ToTable("user_sessions");

        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.Id).HasColumnName("id");
        builder.Property(entity => entity.UserId).HasColumnName("user_id");
        builder.Property(entity => entity.Subject).HasColumnName("subject").HasMaxLength(200);
        builder.Property(entity => entity.ClientId).HasColumnName("client_id").HasMaxLength(100);
        builder.Property(entity => entity.ExpiresAtUtc).HasColumnName("expires_at_utc");
        builder.Property(entity => entity.IsRevoked).HasColumnName("is_revoked");
        builder.Property(entity => entity.RevokedAtUtc).HasColumnName("revoked_at_utc");
        builder.Property(entity => entity.RevokedBy).HasColumnName("revoked_by").HasMaxLength(200);
        builder.Property(entity => entity.RevocationReason).HasColumnName("revocation_reason");
        builder.Property<DateTimeOffset>("CreatedAtUtc").HasColumnName("created_at_utc");
        builder.Property<DateTimeOffset>("UpdatedAtUtc").HasColumnName("updated_at_utc");

        builder.HasIndex(entity => new { entity.UserId, entity.IsRevoked });
        builder.HasIndex(entity => entity.Subject);
    }
}