using Microsoft.EntityFrameworkCore;
using MySso.Application.Common.Interfaces;
using MySso.Contracts.Identity;
using MySso.Contracts.Pagination;
using MySso.Contracts.Security;
using MySso.Infrastructure.Persistence;

namespace MySso.Infrastructure.Services;

public sealed class AdministrationQueryService : IAdministrationQueryService
{
    private readonly MySsoDbContext _dbContext;

    public AdministrationQueryService(MySsoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PageResult<AuditLogEntry>> GetAuditLogsAsync(PageRequest request, CancellationToken cancellationToken)
    {
        var query = _dbContext.AuditLogs
            .AsNoTracking()
            .OrderByDescending(item => item.OccurredAtUtc);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(item => new AuditLogEntry(
                item.Id,
                item.ActorId,
                item.ActionType.ToString(),
                item.ResourceType,
                item.ResourceId,
                item.Succeeded,
                item.OccurredAtUtc,
                item.IpAddress,
                item.Description,
                item.Metadata))
            .ToListAsync(cancellationToken);

        return new PageResult<AuditLogEntry>(items, request.PageNumber, request.PageSize, total);
    }

    public async Task<PageResult<ClientSummary>> GetClientsAsync(PageRequest request, CancellationToken cancellationToken)
    {
        var query = _dbContext.RegisteredClients
            .AsNoTracking()
            .OrderBy(item => item.ClientId);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(item => new ClientSummary(
                item.Id,
                item.ClientId,
                item.DisplayName,
                item.ClientType.ToString(),
                item.RequirePkce,
                item.AllowRefreshTokens,
                item.IsEnabled,
                item.RedirectUris,
                item.AllowedScopes))
            .ToListAsync(cancellationToken);

        return new PageResult<ClientSummary>(items, request.PageNumber, request.PageSize, total);
    }

    public async Task<UserProfileSummary?> GetCurrentUserProfileAsync(string subjectId, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(subjectId, out var identityAccountId))
        {
            return null;
        }

        var account = await _dbContext.IdentityAccounts
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == identityAccountId, cancellationToken);

        if (account is null)
        {
            return null;
        }

        var domainUser = account.DomainUserId is null
            ? null
            : await _dbContext.IdentityUsers.AsNoTracking().SingleOrDefaultAsync(item => item.Id == account.DomainUserId.Value, cancellationToken);

        var roles = await (from userRole in _dbContext.UserRoles.AsNoTracking()
                           join role in _dbContext.IdentityRoles.AsNoTracking() on userRole.RoleId equals role.Id
                           where userRole.UserId == account.Id
                           orderby role.Name
                           select role.Name ?? string.Empty)
            .ToListAsync(cancellationToken);

        return new UserProfileSummary(
            account.Id,
            account.DomainUserId,
            account.Email ?? string.Empty,
            account.GivenName,
            account.FamilyName,
            account.IsActive,
            roles,
            domainUser?.LastSignedInAtUtc,
            domainUser?.LastSignedInIpAddress);
    }

    public async Task<PageResult<RoleSummary>> GetRolesAsync(PageRequest request, CancellationToken cancellationToken)
    {
        var query = _dbContext.DomainRoles
            .AsNoTracking()
            .OrderBy(item => item.Name);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(item => new RoleSummary(item.Id, item.Name, item.Description, item.IsSystemRole))
            .ToListAsync(cancellationToken);

        return new PageResult<RoleSummary>(items, request.PageNumber, request.PageSize, total);
    }

    public async Task<PageResult<UserSessionSummary>> GetSessionsAsync(PageRequest request, CancellationToken cancellationToken)
    {
        var query = _dbContext.UserSessions
            .AsNoTracking()
            .OrderByDescending(item => item.UpdatedAtUtc);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(item => new UserSessionSummary(
                item.Id,
                item.UserId,
                item.Subject,
                item.ClientId,
                item.ExpiresAtUtc,
                item.IsRevoked,
                item.RevokedAtUtc,
                item.RevokedBy,
                item.RevocationReason == null ? null : item.RevocationReason.ToString()))
            .ToListAsync(cancellationToken);

        return new PageResult<UserSessionSummary>(items, request.PageNumber, request.PageSize, total);
    }

    public async Task<PageResult<UserSummary>> GetUsersAsync(PageRequest request, string? searchTerm, CancellationToken cancellationToken)
    {
        var query = _dbContext.IdentityUsers.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLowerInvariant();
            query = query.Where(item =>
                item.Email.Value.Contains(term) ||
                item.GivenName.Value.ToLower().Contains(term) ||
                item.FamilyName.Value.ToLower().Contains(term));
        }

        query = query.OrderBy(item => item.Email.Value);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(item => new UserSummary(item.Id, item.Email.Value, item.GivenName.Value, item.FamilyName.Value, item.IsActive))
            .ToListAsync(cancellationToken);

        return new PageResult<UserSummary>(items, request.PageNumber, request.PageSize, total);
    }
}