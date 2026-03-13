namespace MySso.Contracts.Identity;

public sealed record ClientSummary(
    Guid Id,
    string ClientId,
    string DisplayName,
    string ClientType,
    bool RequirePkce,
    bool AllowRefreshTokens,
    bool IsEnabled,
    IReadOnlyCollection<string> RedirectUris,
    IReadOnlyCollection<string> AllowedScopes);