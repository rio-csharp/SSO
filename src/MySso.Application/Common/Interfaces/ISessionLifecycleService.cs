namespace MySso.Application.Common.Interfaces;

public interface ISessionLifecycleService
{
    Task<Guid> StartInteractiveSessionAsync(
        Guid identityAccountId,
        Guid? domainUserId,
        string subject,
        string? clientId,
        string? ipAddress,
        CancellationToken cancellationToken);

    Task<bool> IsSessionActiveAsync(Guid sessionId, CancellationToken cancellationToken);
}