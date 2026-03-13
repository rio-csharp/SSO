using Microsoft.AspNetCore.Identity;
using MySso.Application.Common.Interfaces;

namespace MySso.Infrastructure.Identity;

public sealed class IdentityAccountProvisioningService : IIdentityAccountProvisioningService
{
    private readonly UserManager<SsoIdentityUser> _userManager;

    public IdentityAccountProvisioningService(UserManager<SsoIdentityUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task ProvisionLocalUserAsync(
        Guid domainUserId,
        string email,
        string givenName,
        string familyName,
        string password,
        IReadOnlyCollection<string> roles,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var existing = await _userManager.FindByEmailAsync(email);

        if (existing is not null)
        {
            throw new InvalidOperationException($"An identity account already exists for '{email}'.");
        }

        var user = new SsoIdentityUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            DomainUserId = domainUserId,
            GivenName = givenName,
            FamilyName = familyName,
            IsActive = true
        };

        var createResult = await _userManager.CreateAsync(user, password);

        if (!createResult.Succeeded)
        {
            throw new InvalidOperationException($"Failed to provision identity account: {string.Join(", ", createResult.Errors.Select(error => error.Description))}");
        }

        if (roles.Count == 0)
        {
            return;
        }

        var roleResult = await _userManager.AddToRolesAsync(user, roles);

        if (!roleResult.Succeeded)
        {
            throw new InvalidOperationException($"Failed to assign roles: {string.Join(", ", roleResult.Errors.Select(error => error.Description))}");
        }
    }
}