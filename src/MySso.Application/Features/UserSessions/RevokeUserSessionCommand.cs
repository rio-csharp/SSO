using MySso.Domain.Enums;

namespace MySso.Application.Features.UserSessions;

public sealed record RevokeUserSessionCommand(Guid SessionId, SessionRevocationReason Reason);