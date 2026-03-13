using MySso.Domain.Entities;

namespace MySso.Application.Common.Interfaces;

public interface IAuditLogRepository
{
    Task AddAsync(AuditLog auditLog, CancellationToken cancellationToken);
}