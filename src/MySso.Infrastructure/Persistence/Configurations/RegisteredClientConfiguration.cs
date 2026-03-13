using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MySso.Domain.Entities;

namespace MySso.Infrastructure.Persistence.Configurations;

internal sealed class RegisteredClientConfiguration : IEntityTypeConfiguration<RegisteredClient>
{
    public void Configure(EntityTypeBuilder<RegisteredClient> builder)
    {
        builder.ToTable("registered_clients");

        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.Id).HasColumnName("id");
        builder.Property(entity => entity.ClientId).HasColumnName("client_id").HasMaxLength(100);
        builder.Property(entity => entity.DisplayName).HasColumnName("display_name").HasMaxLength(200);
        builder.Property(entity => entity.ClientType).HasColumnName("client_type");
        builder.Property(entity => entity.RequirePkce).HasColumnName("require_pkce");
        builder.Property(entity => entity.AllowRefreshTokens).HasColumnName("allow_refresh_tokens");
        builder.Property(entity => entity.RequireConsent).HasColumnName("require_consent");
        builder.Property(entity => entity.IsEnabled).HasColumnName("is_enabled");
        builder.Property<DateTimeOffset>("CreatedAtUtc").HasColumnName("created_at_utc");
        builder.Property<DateTimeOffset>("UpdatedAtUtc").HasColumnName("updated_at_utc");

        builder.Ignore(entity => entity.RedirectUris);
        builder.Ignore(entity => entity.AllowedScopes);
        builder.Property<List<string>>("_redirectUris")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasColumnName("redirect_uris_json")
            .HasConversion(
                value => JsonSerializer.Serialize(value, JsonDefaults.Options),
                value => JsonSerializer.Deserialize<List<string>>(value, JsonDefaults.Options) ?? new List<string>());
        builder.Property<List<string>>("_allowedScopes")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasColumnName("allowed_scopes_json")
            .HasConversion(
                value => JsonSerializer.Serialize(value, JsonDefaults.Options),
                value => JsonSerializer.Deserialize<List<string>>(value, JsonDefaults.Options) ?? new List<string>());

        builder.HasIndex(entity => entity.ClientId).IsUnique();
    }
}