using MySso.Contracts.Identity;
using MySso.Contracts.Pagination;
using MySso.Contracts.Security;

namespace MySso.Application.Common.Interfaces;

public interface IAdministrationQueryService
{
    Task<PageResult<UserSummary>> GetUsersAsync(PageRequest request, string? searchTerm, CancellationToken cancellationToken);

    Task<PageResult<RoleSummary>> GetRolesAsync(PageRequest request, CancellationToken cancellationToken);

    Task<PageResult<ClientSummary>> GetClientsAsync(PageRequest request, CancellationToken cancellationToken);

    Task<PageResult<UserSessionSummary>> GetSessionsAsync(PageRequest request, CancellationToken cancellationToken);

    Task<PageResult<AuditLogEntry>> GetAuditLogsAsync(PageRequest request, CancellationToken cancellationToken);

    Task<UserProfileSummary?> GetCurrentUserProfileAsync(string subjectId, CancellationToken cancellationToken);
}