using MySso.Domain.Entities;
using MySso.Domain.ValueObjects;

namespace MySso.Application.Common.Interfaces;

public interface IUserRepository
{
    Task<bool> ExistsByEmailAsync(EmailAddress email, CancellationToken cancellationToken);

    Task<IdentityUser?> GetByIdAsync(Guid userId, CancellationToken cancellationToken);

    Task AddAsync(IdentityUser user, CancellationToken cancellationToken);

    Task UpdateAsync(IdentityUser user, CancellationToken cancellationToken);
}