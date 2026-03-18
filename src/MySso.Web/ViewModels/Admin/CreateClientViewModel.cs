using System.ComponentModel.DataAnnotations;
using MySso.Domain.Enums;

namespace MySso.Web.ViewModels.Admin;

public sealed class CreateClientViewModel : IValidatableObject
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

    [DataType(DataType.Password)]
    public string? ClientSecret { get; set; }

    [Url]
    public string? PostLogoutRedirectUri { get; set; }

    public ClientType ClientType { get; set; } = ClientType.Confidential;

    public bool AllowRefreshTokens { get; set; } = true;

    public bool RequireConsent { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (ClientType == ClientType.Confidential && string.IsNullOrWhiteSpace(ClientSecret))
        {
            yield return new ValidationResult("Confidential client must provide a client secret.", new[] { nameof(ClientSecret) });
        }
    }
}