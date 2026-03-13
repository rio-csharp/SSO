using MySso.Application.Common.Exceptions;
using MySso.Application.Common.Interfaces;
using MySso.Contracts.Common;
using MySso.Contracts.Security;
using MySso.Domain.Entities;
using MySso.Domain.Enums;

namespace MySso.Application.Features.UserSessions;

public sealed class RevokeUserSessionHandler
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IUserSessionRepository _userSessionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RevokeUserSessionHandler(
        IUserSessionRepository userSessionRepository,
        IAuditLogRepository auditLogRepository,
        ICurrentUserContext currentUserContext,
        IDateTimeProvider dateTimeProvider,
        IUnitOfWork unitOfWork)
    {
        _userSessionRepository = userSessionRepository;
        _auditLogRepository = auditLogRepository;
        _currentUserContext = currentUserContext;
        _dateTimeProvider = dateTimeProvider;
        _unitOfWork = unitOfWork;
    }

    public async Task<OperationResult<UserSessionSummary>> HandleAsync(RevokeUserSessionCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var session = await _userSessionRepository.GetByIdAsync(command.SessionId, cancellationToken);

        if (session is null)
        {
            return OperationResult<UserSessionSummary>.Failure("sessions.not_found", "The session could not be found.");
        }

        EnsureCanRevoke(session);

        var now = _dateTimeProvider.UtcNow;
        session.Revoke(_currentUserContext.SubjectId, command.Reason, now);

        await _userSessionRepository.UpdateAsync(session, cancellationToken);
        await _auditLogRepository.AddAsync(
            AuditLog.Create(
                Guid.NewGuid(),
                _currentUserContext.SubjectId,
                AuditActionType.SessionRevoked,
                nameof(UserSession),
                session.Id.ToString(),
                true,
                now,
                _currentUserContext.IpAddress,
                $"Revoked session {session.Id}.",
                new Dictionary<string, string>
                {
                    ["reason"] = command.Reason.ToString(),
                    ["targetSubject"] = session.Subject
                }),
            cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return OperationResult<UserSessionSummary>.Success(
            new UserSessionSummary(
                session.Id,
                session.UserId,
                session.Subject,
                session.ClientId,
                session.ExpiresAtUtc,
                session.IsRevoked,
                session.RevokedAtUtc,
                session.RevokedBy,
                session.RevocationReason?.ToString()));
    }

    private void EnsureCanRevoke(UserSession session)
    {
        if (!_currentUserContext.IsAuthenticated)
        {
            throw new ForbiddenAccessException("Authenticated users are required to revoke sessions.");
        }

        var isOwner = string.Equals(_currentUserContext.SubjectId, session.Subject, StringComparison.Ordinal);
        var isAdministrator = _currentUserContext.IsInRole("Administrator");

        if (!isOwner && !isAdministrator)
        {
            throw new ForbiddenAccessException("Only the session owner or an administrator can revoke the session.");
        }
    }
}