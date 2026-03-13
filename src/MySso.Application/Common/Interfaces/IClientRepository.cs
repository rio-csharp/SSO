using MySso.Domain.Entities;

namespace MySso.Application.Common.Interfaces;

public interface IClientRepository
{
    Task<bool> ExistsByClientIdAsync(string clientId, CancellationToken cancellationToken);

    Task AddAsync(RegisteredClient client, CancellationToken cancellationToken);
}