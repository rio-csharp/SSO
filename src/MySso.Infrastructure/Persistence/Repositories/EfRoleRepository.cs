using Microsoft.EntityFrameworkCore;
using MySso.Application.Common.Interfaces;
using MySso.Domain.Entities;

namespace MySso.Infrastructure.Persistence.Repositories;

public sealed class EfRoleRepository : IRoleRepository
{
    private readonly MySsoDbContext _dbContext;

    public EfRoleRepository(MySsoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task AddAsync(Role role, CancellationToken cancellationToken)
        => _dbContext.DomainRoles.AddAsync(role, cancellationToken).AsTask();

    public Task<bool> ExistsByNameAsync(string roleName, CancellationToken cancellationToken)
        => _dbContext.DomainRoles.AnyAsync(role => role.Name == roleName, cancellationToken);
}