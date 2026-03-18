using MySso.Domain.Entities;

namespace MySso.Application.Common.Interfaces;

public interface IClientProvisioningService
{
    Task<bool> ExistsByClientIdAsync(string clientId, CancellationToken cancellationToken);

    Task ProvisionAsync(RegisteredClient client, string? clientSecret, string? postLogoutRedirectUri, CancellationToken cancellationToken);
}