using MySso.Domain.Entities;

namespace MySso.Application.Common.Interfaces;

public interface IUserSessionRepository
{
    Task<UserSession?> GetByIdAsync(Guid sessionId, CancellationToken cancellationToken);

    Task AddAsync(UserSession session, CancellationToken cancellationToken);

    Task UpdateAsync(UserSession session, CancellationToken cancellationToken);
}