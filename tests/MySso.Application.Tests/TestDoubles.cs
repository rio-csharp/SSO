using MySso.Application.Common.Interfaces;
using MySso.Domain.Entities;

namespace MySso.Application.Tests;

internal sealed class FakeAuditLogRepository : IAuditLogRepository
{
    public List<AuditLog> AuditLogs { get; } = new();

    public Task AddAsync(AuditLog auditLog, CancellationToken cancellationToken)
    {
        AuditLogs.Add(auditLog);
        return Task.CompletedTask;
    }
}

internal sealed class FakeClientRepository : IClientRepository
{
    public List<RegisteredClient> Clients { get; } = new();

    public bool ExistsResult { get; set; }

    public Task AddAsync(RegisteredClient client, CancellationToken cancellationToken)
    {
        Clients.Add(client);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsByClientIdAsync(string clientId, CancellationToken cancellationToken)
        => Task.FromResult(ExistsResult || Clients.Any(client => string.Equals(client.ClientId, clientId, StringComparison.OrdinalIgnoreCase)));
}

internal sealed class FakeClientProvisioningService : IClientProvisioningService
{
    public bool ExistsResult { get; set; }

    public int ProvisionCallCount { get; private set; }

    public string? LastClientSecret { get; private set; }

    public string? LastPostLogoutRedirectUri { get; private set; }

    public Task<bool> ExistsByClientIdAsync(string clientId, CancellationToken cancellationToken)
        => Task.FromResult(ExistsResult);

    public Task ProvisionAsync(RegisteredClient client, string? clientSecret, string? postLogoutRedirectUri, CancellationToken cancellationToken)
    {
        ProvisionCallCount++;
        LastClientSecret = clientSecret;
        LastPostLogoutRedirectUri = postLogoutRedirectUri;
        return Task.CompletedTask;
    }
}

internal sealed class FakeUserSessionRepository : IUserSessionRepository
{
    private readonly Dictionary<Guid, UserSession> _sessions;

    public FakeUserSessionRepository(UserSession session)
    {
        _sessions = new Dictionary<Guid, UserSession>
        {
            [session.Id] = session
        };
    }

    public Task<UserSession?> GetByIdAsync(Guid sessionId, CancellationToken cancellationToken)
        => Task.FromResult(_sessions.TryGetValue(sessionId, out var session) ? session : null);

    public Task AddAsync(UserSession session, CancellationToken cancellationToken)
    {
        _sessions[session.Id] = session;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(UserSession session, CancellationToken cancellationToken)
    {
        _sessions[session.Id] = session;
        return Task.CompletedTask;
    }
}

internal sealed class FakeUnitOfWork : IUnitOfWork
{
    public int SaveChangesCallCount { get; private set; }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        SaveChangesCallCount++;
        return Task.CompletedTask;
    }
}

internal sealed class FakeDateTimeProvider : IDateTimeProvider
{
    public FakeDateTimeProvider(DateTimeOffset utcNow)
    {
        UtcNow = utcNow;
    }

    public DateTimeOffset UtcNow { get; }
}

internal sealed class FakeCurrentUserContext : ICurrentUserContext
{
    private readonly HashSet<string> _roles;

    private FakeCurrentUserContext(bool isAuthenticated, string subjectId, string? displayName, string? ipAddress, IEnumerable<string> roles)
    {
        IsAuthenticated = isAuthenticated;
        SubjectId = subjectId;
        DisplayName = displayName;
        IpAddress = ipAddress;
        _roles = new HashSet<string>(roles, StringComparer.OrdinalIgnoreCase);
    }

    public bool IsAuthenticated { get; }

    public string SubjectId { get; }

    public string? DisplayName { get; }

    public string? IpAddress { get; }

    public bool IsInRole(string roleName) => _roles.Contains(roleName);

    public static FakeCurrentUserContext Administrator()
        => new(true, "admin-1", "Admin", "127.0.0.1", new[] { "Administrator" });

    public static FakeCurrentUserContext AuthenticatedUser(string subjectId)
        => new(true, subjectId, subjectId, "127.0.0.1", Array.Empty<string>());
}