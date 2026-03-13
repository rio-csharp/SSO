using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MySso.Domain.Entities;
using MySso.Domain.Enums;
using MySso.Domain.ValueObjects;
using MySso.Infrastructure.Identity;
using MySso.Infrastructure.Options;
using MySso.Infrastructure.Persistence;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;
using DomainIdentityUser = MySso.Domain.Entities.IdentityUser;

namespace MySso.Infrastructure.Bootstrap;

public static class InfrastructureInitializer
{
    public static async Task InitializeInfrastructureAsync(this IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<MySsoDbContext>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<SsoIdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<SsoIdentityUser>>();
        var applicationManager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();
        var bootstrapOptions = scope.ServiceProvider.GetRequiredService<IOptions<MySsoBootstrapOptions>>().Value;

        await dbContext.Database.EnsureCreatedAsync(cancellationToken);

        var domainRole = await EnsureDomainAdministratorRoleAsync(dbContext, cancellationToken);
        await EnsureIdentityAdministratorRoleAsync(roleManager, domainRole, cancellationToken);

        var domainUser = await EnsureDomainAdministratorUserAsync(dbContext, bootstrapOptions, cancellationToken);
        var identityUser = await EnsureIdentityAdministratorUserAsync(userManager, bootstrapOptions, domainUser, cancellationToken);

        if (!await userManager.IsInRoleAsync(identityUser, "Administrator"))
        {
            await userManager.AddToRoleAsync(identityUser, "Administrator");
        }

        await EnsureSampleClientAsync(applicationManager, bootstrapOptions, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static async Task<Role> EnsureDomainAdministratorRoleAsync(MySsoDbContext dbContext, CancellationToken cancellationToken)
    {
        var role = await dbContext.DomainRoles.SingleOrDefaultAsync(item => item.Name == "Administrator", cancellationToken);

        if (role is not null)
        {
            return role;
        }

        role = Role.Create(Guid.NewGuid(), "Administrator", "Built-in system administrator role.", true, DateTimeOffset.UtcNow);
        await dbContext.DomainRoles.AddAsync(role, cancellationToken);

        return role;
    }

    private static async Task EnsureIdentityAdministratorRoleAsync(RoleManager<SsoIdentityRole> roleManager, Role domainRole, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var role = await roleManager.FindByNameAsync("Administrator");

        if (role is not null)
        {
            return;
        }

        role = new SsoIdentityRole
        {
            Name = "Administrator",
            NormalizedName = "ADMINISTRATOR",
            Description = domainRole.Description,
            DomainRoleId = domainRole.Id,
            IsSystemRole = true
        };

        var result = await roleManager.CreateAsync(role);

        if (!result.Succeeded)
        {
            throw new InvalidOperationException($"Failed to create administrator role: {string.Join(", ", result.Errors.Select(error => error.Description))}");
        }
    }

    private static async Task<DomainIdentityUser> EnsureDomainAdministratorUserAsync(MySsoDbContext dbContext, MySsoBootstrapOptions options, CancellationToken cancellationToken)
    {
        var email = new EmailAddress(options.AdminEmail);
        var existing = await dbContext.IdentityUsers.SingleOrDefaultAsync(item => item.Email == email, cancellationToken);

        if (existing is not null)
        {
            return existing;
        }

        var user = DomainIdentityUser.Create(
            Guid.NewGuid(),
            email,
            new PersonName(options.AdminGivenName),
            new PersonName(options.AdminFamilyName),
            DateTimeOffset.UtcNow);

        await dbContext.IdentityUsers.AddAsync(user, cancellationToken);

        return user;
    }

    private static async Task<SsoIdentityUser> EnsureIdentityAdministratorUserAsync(
        UserManager<SsoIdentityUser> userManager,
        MySsoBootstrapOptions options,
        DomainIdentityUser domainUser,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = await userManager.FindByEmailAsync(options.AdminEmail);

        if (user is not null)
        {
            return user;
        }

        user = new SsoIdentityUser
        {
            UserName = options.AdminEmail,
            Email = options.AdminEmail,
            EmailConfirmed = true,
            GivenName = options.AdminGivenName,
            FamilyName = options.AdminFamilyName,
            IsActive = true,
            DomainUserId = domainUser.Id
        };

        var result = await userManager.CreateAsync(user, options.AdminPassword);

        if (!result.Succeeded)
        {
            throw new InvalidOperationException($"Failed to create administrator user: {string.Join(", ", result.Errors.Select(error => error.Description))}");
        }

        return user;
    }

    private static async Task EnsureSampleClientAsync(
        IOpenIddictApplicationManager applicationManager,
        MySsoBootstrapOptions options,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (await applicationManager.FindByClientIdAsync(options.ClientId, cancellationToken) is not null)
        {
            return;
        }

        var descriptor = new OpenIddictApplicationDescriptor
        {
            ClientId = options.ClientId,
            ClientSecret = options.ClientSecret,
            ClientType = ClientTypes.Confidential,
            ConsentType = ConsentTypes.Implicit,
            DisplayName = "Sample MVC Client",
            RedirectUris = { new Uri(options.RedirectUri) },
            PostLogoutRedirectUris = { new Uri(options.PostLogoutRedirectUri) },
            Permissions =
            {
                Permissions.Endpoints.Authorization,
                Permissions.Endpoints.EndSession,
                Permissions.Endpoints.Token,
                Permissions.GrantTypes.AuthorizationCode,
                Permissions.GrantTypes.RefreshToken,
                Permissions.ResponseTypes.Code,
                Permissions.Scopes.Email,
                Permissions.Scopes.Profile,
                Permissions.Prefixes.Scope + Scopes.OfflineAccess,
                Permissions.Prefixes.Scope + Scopes.OpenId,
                Permissions.Prefixes.Scope + "api"
            },
            Requirements =
            {
                Requirements.Features.ProofKeyForCodeExchange
            }
        };

        await applicationManager.CreateAsync(descriptor, cancellationToken);
    }
}