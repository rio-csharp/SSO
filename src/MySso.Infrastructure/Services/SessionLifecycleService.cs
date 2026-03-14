using Microsoft.Extensions.Options;
using MySso.Application.Common.Interfaces;
using MySso.Domain.Entities;
using MySso.Domain.Enums;
using MySso.Infrastructure.Options;

namespace MySso.Infrastructure.Services;

public sealed class SessionLifecycleService : ISessionLifecycleService
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly MySsoHostOptions _hostOptions;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserRepository _userRepository;
    private readonly IUserSessionRepository _userSessionRepository;

    public SessionLifecycleService(
        IUserRepository userRepository,
        IUserSessionRepository userSessionRepository,
        IAuditLogRepository auditLogRepository,
        IDateTimeProvider dateTimeProvider,
        IUnitOfWork unitOfWork,
        IOptions<MySsoHostOptions> hostOptions)
    {
        _userRepository = userRepository;
        _userSessionRepository = userSessionRepository;
        _auditLogRepository = auditLogRepository;
        _dateTimeProvider = dateTimeProvider;
        _unitOfWork = unitOfWork;
        _hostOptions = hostOptions.Value;
    }

    public async Task<Guid> StartInteractiveSessionAsync(
        Guid identityAccountId,
        Guid? domainUserId,
        string subject,
        string? clientId,
        string? ipAddress,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var now = _dateTimeProvider.UtcNow;
        var persistedUserId = domainUserId ?? identityAccountId;

        if (domainUserId.HasValue)
        {
            var domainUser = await _userRepository.GetByIdAsync(domainUserId.Value, cancellationToken);

            if (domainUser is not null)
            {
                domainUser.RecordSuccessfulSignIn(ipAddress ?? "unknown", now);
                await _userRepository.UpdateAsync(domainUser, cancellationToken);
            }
        }

        var session = UserSession.Start(
            Guid.NewGuid(),
            persistedUserId,
            subject,
            clientId,
            now,
            now.AddDays(_hostOptions.RefreshTokenLifetimeDays));

        await _userSessionRepository.AddAsync(session, cancellationToken);
        await _auditLogRepository.AddAsync(
            AuditLog.Create(
                Guid.NewGuid(),
                subject,
                AuditActionType.SessionStarted,
                nameof(UserSession),
                session.Id.ToString(),
                true,
                now,
                ipAddress,
                $"Started session {session.Id}.",
                new Dictionary<string, string>
                {
                    ["clientId"] = clientId ?? string.Empty,
                    ["subject"] = subject
                }),
            cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return session.Id;
    }

    public async Task<bool> IsSessionActiveAsync(Guid sessionId, CancellationToken cancellationToken)
    {
        var session = await _userSessionRepository.GetByIdAsync(sessionId, cancellationToken);
        return session?.IsActiveAt(_dateTimeProvider.UtcNow) == true;
    }
}