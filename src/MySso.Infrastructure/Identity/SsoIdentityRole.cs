using Microsoft.AspNetCore.Identity;

namespace MySso.Infrastructure.Identity;

public sealed class SsoIdentityRole : IdentityRole<Guid>
{
    public Guid? DomainRoleId { get; set; }

    public string Description { get; set; } = string.Empty;

    public bool IsSystemRole { get; set; }
}