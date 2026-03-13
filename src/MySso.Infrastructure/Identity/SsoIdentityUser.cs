using Microsoft.AspNetCore.Identity;

namespace MySso.Infrastructure.Identity;

public sealed class SsoIdentityUser : IdentityUser<Guid>
{
    public Guid? DomainUserId { get; set; }

    public string GivenName { get; set; } = string.Empty;

    public string FamilyName { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}