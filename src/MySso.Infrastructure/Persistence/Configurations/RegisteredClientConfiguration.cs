using System.Text.Json;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MySso.Domain.Entities;

namespace MySso.Infrastructure.Persistence.Configurations;

internal sealed class RegisteredClientConfiguration : IEntityTypeConfiguration<RegisteredClient>
{
    private static readonly ValueComparer<List<string>> StringListComparer = new(
        (left, right) => ListEquals(left, right),
        value => GetListHashCode(value),
        value => value.ToList());

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
                value => JsonSerializer.Deserialize<List<string>>(value, JsonDefaults.Options) ?? new List<string>())
            .Metadata.SetValueComparer(StringListComparer);
        builder.Property<List<string>>("_allowedScopes")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasColumnName("allowed_scopes_json")
            .HasConversion(
                value => JsonSerializer.Serialize(value, JsonDefaults.Options),
                value => JsonSerializer.Deserialize<List<string>>(value, JsonDefaults.Options) ?? new List<string>())
            .Metadata.SetValueComparer(StringListComparer);

        builder.HasIndex(entity => entity.ClientId).IsUnique();
    }

    private static bool ListEquals(List<string>? left, List<string>? right)
    {
        if (ReferenceEquals(left, right))
        {
            return true;
        }

        if (left is null || right is null || left.Count != right.Count)
        {
            return false;
        }

        for (var index = 0; index < left.Count; index++)
        {
            if (!string.Equals(left[index], right[index], StringComparison.Ordinal))
            {
                return false;
            }
        }

        return true;
    }

    private static int GetListHashCode(List<string>? value)
    {
        if (value is null)
        {
            return 0;
        }

        var hash = new HashCode();

        foreach (var item in value)
        {
            hash.Add(item, StringComparer.Ordinal);
        }

        return hash.ToHashCode();
    }
}