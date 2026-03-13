using MySso.Application.Common.Exceptions;
using MySso.Application.Common.Interfaces;
using MySso.Contracts.Common;
using MySso.Contracts.Identity;
using MySso.Domain.Entities;
using MySso.Domain.Enums;
using MySso.Domain.ValueObjects;

namespace MySso.Application.Features.IdentityUsers;

public sealed class CreateUserHandler
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserRepository _userRepository;

    public CreateUserHandler(
        IUserRepository userRepository,
        IAuditLogRepository auditLogRepository,
        ICurrentUserContext currentUserContext,
        IDateTimeProvider dateTimeProvider,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _auditLogRepository = auditLogRepository;
        _currentUserContext = currentUserContext;
        _dateTimeProvider = dateTimeProvider;
        _unitOfWork = unitOfWork;
    }

    public async Task<OperationResult<UserSummary>> HandleAsync(CreateUserCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        EnsureAdministrativeAccess();

        var email = new EmailAddress(command.Email);

        if (await _userRepository.ExistsByEmailAsync(email, cancellationToken))
        {
            return OperationResult<UserSummary>.Failure("users.duplicate_email", "A user with the same email already exists.");
        }

        var now = _dateTimeProvider.UtcNow;
        var user = IdentityUser.Create(
            Guid.NewGuid(),
            email,
            new PersonName(command.GivenName),
            new PersonName(command.FamilyName),
            now);

        await _userRepository.AddAsync(user, cancellationToken);
        await _auditLogRepository.AddAsync(
            AuditLog.Create(
                Guid.NewGuid(),
                ResolveActorId(),
                AuditActionType.UserCreated,
                nameof(IdentityUser),
                user.Id.ToString(),
                true,
                now,
                _currentUserContext.IpAddress,
                $"Created user {user.DisplayName}.",
                new Dictionary<string, string>
                {
                    ["email"] = user.Email.Value
                }),
            cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return OperationResult<UserSummary>.Success(new UserSummary(user.Id, user.Email.Value, user.GivenName.Value, user.FamilyName.Value, user.IsActive));
    }

    private void EnsureAdministrativeAccess()
    {
        if (!_currentUserContext.IsAuthenticated || !_currentUserContext.IsInRole("Administrator"))
        {
            throw new ForbiddenAccessException("Only administrators can create users.");
        }
    }

    private string ResolveActorId()
        => _currentUserContext.SubjectId;
}