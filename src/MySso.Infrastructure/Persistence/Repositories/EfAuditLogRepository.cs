using MySso.Application.Common.Interfaces;
using MySso.Domain.Entities;

namespace MySso.Infrastructure.Persistence.Repositories;

public sealed class EfAuditLogRepository : IAuditLogRepository
{
    private readonly MySsoDbContext _dbContext;

    public EfAuditLogRepository(MySsoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task AddAsync(AuditLog auditLog, CancellationToken cancellationToken)
        => _dbContext.AuditLogs.AddAsync(auditLog, cancellationToken).AsTask();
}