using MySso.Domain.Entities;

namespace MySso.Application.Common.Interfaces;

public interface IRoleRepository
{
    Task<bool> ExistsByNameAsync(string roleName, CancellationToken cancellationToken);

    Task AddAsync(Role role, CancellationToken cancellationToken);
}