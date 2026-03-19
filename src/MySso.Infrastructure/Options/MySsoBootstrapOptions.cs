using System.ComponentModel.DataAnnotations;

namespace MySso.Infrastructure.Options;

public sealed class MySsoBootstrapOptions
{
    public const string SectionName = "Bootstrap";

    [Required]
    [EmailAddress]
    public string AdminEmail { get; set; } = "admin@mysso.local";

    [Required]
    [MinLength(12)]
    public string AdminPassword { get; set; } = string.Empty;

    [Required]
    public string AdminGivenName { get; set; } = "System";

    [Required]
    public string AdminFamilyName { get; set; } = "Administrator";

    [Required]
    public string ClientId { get; set; } = "sample-client-web";

    [Required]
    public string ClientSecret { get; set; } = string.Empty;

    [Required]
    [Url]
    public string RedirectUri { get; set; } = "https://localhost:7041/signin-oidc";

    [Required]
    [Url]
    public string PostLogoutRedirectUri { get; set; } = "https://localhost:7041/signout-callback-oidc";
}