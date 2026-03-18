using MySso.Application.Common.Exceptions;
using MySso.Application.Common.Interfaces;
using MySso.Contracts.Common;
using MySso.Contracts.Identity;
using MySso.Domain.Entities;
using MySso.Domain.Enums;

namespace MySso.Application.Features.Clients;

public sealed class RegisterClientHandler
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IClientRepository _clientRepository;
    private readonly IClientProvisioningService _clientProvisioningService;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterClientHandler(
        IClientRepository clientRepository,
        IClientProvisioningService clientProvisioningService,
        IAuditLogRepository auditLogRepository,
        ICurrentUserContext currentUserContext,
        IDateTimeProvider dateTimeProvider,
        IUnitOfWork unitOfWork)
    {
        _clientRepository = clientRepository;
        _clientProvisioningService = clientProvisioningService;
        _auditLogRepository = auditLogRepository;
        _currentUserContext = currentUserContext;
        _dateTimeProvider = dateTimeProvider;
        _unitOfWork = unitOfWork;
    }

    public async Task<OperationResult<ClientSummary>> HandleAsync(RegisterClientCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        EnsureAdministrativeAccess();

        if (await _clientRepository.ExistsByClientIdAsync(command.ClientId, cancellationToken)
            || await _clientProvisioningService.ExistsByClientIdAsync(command.ClientId, cancellationToken))
        {
            return OperationResult<ClientSummary>.Failure("clients.duplicate_client_id", "A client with the same client id already exists.");
        }

        var now = _dateTimeProvider.UtcNow;
        var client = RegisteredClient.Create(
            Guid.NewGuid(),
            command.ClientId,
            command.DisplayName,
            command.ClientType,
            command.RequirePkce,
            command.AllowRefreshTokens,
            command.RequireConsent,
            command.RedirectUris,
            command.AllowedScopes,
            now);

        await _clientRepository.AddAsync(client, cancellationToken);
        await _clientProvisioningService.ProvisionAsync(client, command.ClientSecret, command.PostLogoutRedirectUri, cancellationToken);
        await _auditLogRepository.AddAsync(
            AuditLog.Create(
                Guid.NewGuid(),
                _currentUserContext.SubjectId,
                AuditActionType.ClientRegistered,
                nameof(RegisteredClient),
                client.Id.ToString(),
                true,
                now,
                _currentUserContext.IpAddress,
                $"Registered client {client.ClientId}.",
                new Dictionary<string, string>
                {
                    ["clientId"] = client.ClientId,
                    ["clientType"] = client.ClientType.ToString()
                }),
            cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return OperationResult<ClientSummary>.Success(
            new ClientSummary(
                client.Id,
                client.ClientId,
                client.DisplayName,
                client.ClientType.ToString(),
                client.RequirePkce,
                client.AllowRefreshTokens,
                client.IsEnabled,
                client.RedirectUris,
                client.AllowedScopes));
    }

    private void EnsureAdministrativeAccess()
    {
        if (!_currentUserContext.IsAuthenticated || !_currentUserContext.IsInRole("Administrator"))
        {
            throw new ForbiddenAccessException("Only administrators can register clients.");
        }
    }
}