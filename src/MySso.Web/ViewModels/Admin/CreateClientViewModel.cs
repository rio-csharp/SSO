using System.ComponentModel.DataAnnotations;
using MySso.Domain.Enums;

namespace MySso.Web.ViewModels.Admin;

public sealed class CreateClientViewModel
{
    [Required]
    public string ClientId { get; set; } = string.Empty;

    [Required]
    public string DisplayName { get; set; } = string.Empty;

    [Required]
    [Url]
    public string RedirectUri { get; set; } = string.Empty;

    [Required]
    public string AllowedScopes { get; set; } = "openid profile email api offline_access";

    public ClientType ClientType { get; set; } = ClientType.Confidential;

    public bool AllowRefreshTokens { get; set; } = true;

    public bool RequireConsent { get; set; }
}