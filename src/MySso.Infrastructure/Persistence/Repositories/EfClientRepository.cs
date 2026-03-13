using Microsoft.EntityFrameworkCore;
using MySso.Application.Common.Interfaces;
using MySso.Domain.Entities;

namespace MySso.Infrastructure.Persistence.Repositories;

public sealed class EfClientRepository : IClientRepository
{
    private readonly MySsoDbContext _dbContext;

    public EfClientRepository(MySsoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task AddAsync(RegisteredClient client, CancellationToken cancellationToken)
        => _dbContext.RegisteredClients.AddAsync(client, cancellationToken).AsTask();

    public Task<bool> ExistsByClientIdAsync(string clientId, CancellationToken cancellationToken)
        => _dbContext.RegisteredClients.AnyAsync(client => client.ClientId == clientId, cancellationToken);
}