using System.ComponentModel.DataAnnotations;

namespace MySso.Infrastructure.Options;

public sealed class MySsoHostOptions
{
    public const string SectionName = "MySso";

    [Required]
    [Url]
    public string Issuer { get; set; } = "https://localhost:5001";

    [Required]
    [MinLength(3)]
    public string CookieName { get; set; } = "MySso.Auth";

    public bool RequireHttps { get; set; } = true;

    [Range(1, 1440)]
    public int AccessTokenLifetimeMinutes { get; set; } = 15;

    [Range(1, 365)]
    public int RefreshTokenLifetimeDays { get; set; } = 14;
}