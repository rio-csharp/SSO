using MySso.Domain.Enums;

namespace MySso.Application.Features.Clients;

public sealed record RegisterClientCommand(
    string ClientId,
    string DisplayName,
    ClientType ClientType,
    bool RequirePkce,
    bool AllowRefreshTokens,
    bool RequireConsent,
    IReadOnlyCollection<string> RedirectUris,
    IReadOnlyCollection<string> AllowedScopes);