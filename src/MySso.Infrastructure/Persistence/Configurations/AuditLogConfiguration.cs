using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MySso.Domain.Entities;

namespace MySso.Infrastructure.Persistence.Configurations;

internal sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("audit_logs");

        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.Id).HasColumnName("id");
        builder.Property(entity => entity.ActorId).HasColumnName("actor_id").HasMaxLength(200);
        builder.Property(entity => entity.ActionType).HasColumnName("action_type");
        builder.Property(entity => entity.ResourceType).HasColumnName("resource_type").HasMaxLength(200);
        builder.Property(entity => entity.ResourceId).HasColumnName("resource_id").HasMaxLength(200);
        builder.Property(entity => entity.Succeeded).HasColumnName("succeeded");
        builder.Property(entity => entity.OccurredAtUtc).HasColumnName("occurred_at_utc");
        builder.Property(entity => entity.IpAddress).HasColumnName("ip_address").HasMaxLength(64);
        builder.Property(entity => entity.Description).HasColumnName("description").HasMaxLength(1000);

        builder.Ignore(entity => entity.Metadata);
        builder.Property<Dictionary<string, string>>("_metadata")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasColumnName("metadata_json")
            .HasConversion(
                value => JsonSerializer.Serialize(value, JsonDefaults.Options),
                value => JsonSerializer.Deserialize<Dictionary<string, string>>(value, JsonDefaults.Options) ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));

        builder.HasIndex(entity => entity.OccurredAtUtc);
        builder.HasIndex(entity => new { entity.ResourceType, entity.ResourceId });
    }
}