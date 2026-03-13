using MySso.Application.Common.Exceptions;
using MySso.Application.Common.Interfaces;
using MySso.Contracts.Common;
using MySso.Contracts.Identity;
using MySso.Domain.Entities;
using MySso.Domain.Enums;

namespace MySso.Application.Features.Roles;

public sealed class CreateRoleHandler
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IRoleRepository _roleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateRoleHandler(
        IRoleRepository roleRepository,
        IAuditLogRepository auditLogRepository,
        ICurrentUserContext currentUserContext,
        IDateTimeProvider dateTimeProvider,
        IUnitOfWork unitOfWork)
    {
        _roleRepository = roleRepository;
        _auditLogRepository = auditLogRepository;
        _currentUserContext = currentUserContext;
        _dateTimeProvider = dateTimeProvider;
        _unitOfWork = unitOfWork;
    }

    public async Task<OperationResult<RoleSummary>> HandleAsync(CreateRoleCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        EnsureAdministrativeAccess();

        if (await _roleRepository.ExistsByNameAsync(command.Name, cancellationToken))
        {
            return OperationResult<RoleSummary>.Failure("roles.duplicate_name", "A role with the same name already exists.");
        }

        var now = _dateTimeProvider.UtcNow;
        var role = Role.Create(Guid.NewGuid(), command.Name, command.Description, command.IsSystemRole, now);

        await _roleRepository.AddAsync(role, cancellationToken);
        await _auditLogRepository.AddAsync(
            AuditLog.Create(
                Guid.NewGuid(),
                _currentUserContext.SubjectId,
                AuditActionType.RoleCreated,
                nameof(Role),
                role.Id.ToString(),
                true,
                now,
                _currentUserContext.IpAddress,
                $"Created role {role.Name}.",
                new Dictionary<string, string>
                {
                    ["roleName"] = role.Name
                }),
            cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return OperationResult<RoleSummary>.Success(new RoleSummary(role.Id, role.Name, role.Description, role.IsSystemRole));
    }

    private void EnsureAdministrativeAccess()
    {
        if (!_currentUserContext.IsAuthenticated || !_currentUserContext.IsInRole("Administrator"))
        {
            throw new ForbiddenAccessException("Only administrators can create roles.");
        }
    }
}