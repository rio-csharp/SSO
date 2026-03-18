using MySso.Application.Common.Interfaces;
using MySso.Domain.Entities;
using MySso.Domain.Enums;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace MySso.Infrastructure.Services;

public sealed class OpenIddictClientProvisioningService : IClientProvisioningService
{
    private readonly IOpenIddictApplicationManager _applicationManager;

    public OpenIddictClientProvisioningService(IOpenIddictApplicationManager applicationManager)
    {
        _applicationManager = applicationManager;
    }

    public async Task<bool> ExistsByClientIdAsync(string clientId, CancellationToken cancellationToken)
        => await _applicationManager.FindByClientIdAsync(clientId, cancellationToken) is not null;

    public async Task ProvisionAsync(RegisteredClient client, string? clientSecret, string? postLogoutRedirectUri, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(client);

        if (client.ClientType == ClientType.Confidential && string.IsNullOrWhiteSpace(clientSecret))
        {
            throw new InvalidOperationException("Confidential clients require a client secret.");
        }

        var descriptor = new OpenIddictApplicationDescriptor
        {
            ClientId = client.ClientId,
            ClientType = client.ClientType == ClientType.Confidential ? ClientTypes.Confidential : ClientTypes.Public,
            ClientSecret = client.ClientType == ClientType.Confidential ? clientSecret : null,
            ConsentType = client.RequireConsent ? ConsentTypes.Explicit : ConsentTypes.Implicit,
            DisplayName = client.DisplayName
        };

        foreach (var redirectUri in client.RedirectUris)
        {
            descriptor.RedirectUris.Add(new Uri(redirectUri));
        }

        if (!string.IsNullOrWhiteSpace(postLogoutRedirectUri))
        {
            descriptor.PostLogoutRedirectUris.Add(new Uri(postLogoutRedirectUri));
        }

        descriptor.Permissions.Add(Permissions.Endpoints.Authorization);
        descriptor.Permissions.Add(Permissions.Endpoints.EndSession);
        descriptor.Permissions.Add(Permissions.Endpoints.Token);
        descriptor.Permissions.Add(Permissions.GrantTypes.AuthorizationCode);
        descriptor.Permissions.Add(Permissions.ResponseTypes.Code);

        if (client.AllowRefreshTokens)
        {
            descriptor.Permissions.Add(Permissions.GrantTypes.RefreshToken);
        }

        foreach (var scope in client.AllowedScopes)
        {
            descriptor.Permissions.Add(Permissions.Prefixes.Scope + scope);
        }

        if (client.RequirePkce)
        {
            descriptor.Requirements.Add(Requirements.Features.ProofKeyForCodeExchange);
        }

        await _applicationManager.CreateAsync(descriptor, cancellationToken);
    }
}