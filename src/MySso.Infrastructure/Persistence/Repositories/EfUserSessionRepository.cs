using Microsoft.EntityFrameworkCore;
using MySso.Application.Common.Interfaces;
using MySso.Domain.Entities;

namespace MySso.Infrastructure.Persistence.Repositories;

public sealed class EfUserSessionRepository : IUserSessionRepository
{
    private readonly MySsoDbContext _dbContext;

    public EfUserSessionRepository(MySsoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<UserSession?> GetByIdAsync(Guid sessionId, CancellationToken cancellationToken)
        => _dbContext.UserSessions.SingleOrDefaultAsync(session => session.Id == sessionId, cancellationToken);

    public Task AddAsync(UserSession session, CancellationToken cancellationToken)
        => _dbContext.UserSessions.AddAsync(session, cancellationToken).AsTask();

    public Task UpdateAsync(UserSession session, CancellationToken cancellationToken)
    {
        _dbContext.UserSessions.Update(session);
        return Task.CompletedTask;
    }
}