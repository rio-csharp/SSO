using Microsoft.EntityFrameworkCore;
using MySso.Application.Common.Interfaces;
using MySso.Domain.Entities;
using MySso.Domain.ValueObjects;

namespace MySso.Infrastructure.Persistence.Repositories;

public sealed class EfUserRepository : IUserRepository
{
    private readonly MySsoDbContext _dbContext;

    public EfUserRepository(MySsoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task AddAsync(IdentityUser user, CancellationToken cancellationToken)
        => _dbContext.IdentityUsers.AddAsync(user, cancellationToken).AsTask();

    public Task<bool> ExistsByEmailAsync(EmailAddress email, CancellationToken cancellationToken)
        => _dbContext.IdentityUsers.AnyAsync(user => user.Email == email, cancellationToken);

    public Task<IdentityUser?> GetByIdAsync(Guid userId, CancellationToken cancellationToken)
        => _dbContext.IdentityUsers.SingleOrDefaultAsync(user => user.Id == userId, cancellationToken);

    public Task UpdateAsync(IdentityUser user, CancellationToken cancellationToken)
    {
        _dbContext.IdentityUsers.Update(user);
        return Task.CompletedTask;
    }
}