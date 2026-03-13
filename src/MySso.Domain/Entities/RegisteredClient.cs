using MySso.Domain.Common;
using MySso.Domain.Enums;

namespace MySso.Domain.Entities;

public sealed class RegisteredClient : AuditableEntity
{
    private List<string> _allowedScopes = new();
    private List<string> _redirectUris = new();

    private RegisteredClient()
    {
    }

    private RegisteredClient(
        Guid id,
        string clientId,
        string displayName,
        ClientType clientType,
        bool requirePkce,
        bool allowRefreshTokens,
        bool requireConsent,
        IEnumerable<string> redirectUris,
        IEnumerable<string> allowedScopes,
        DateTimeOffset createdAtUtc)
        : base(id, createdAtUtc)
    {
        ClientId = NormalizeIdentifier(clientId, nameof(clientId));
        DisplayName = NormalizeDisplayName(displayName);
        ClientType = clientType;
        RequirePkce = requirePkce;
        AllowRefreshTokens = allowRefreshTokens;
        RequireConsent = requireConsent;
        _redirectUris = NormalizeRedirectUris(redirectUris).ToList();
        _allowedScopes = NormalizeScopes(allowedScopes).ToList();
        IsEnabled = true;

        if (!RequirePkce)
        {
            throw new ArgumentException("Authorization code clients must require PKCE.", nameof(requirePkce));
        }
    }

    public string ClientId { get; private set; } = string.Empty;

    public string DisplayName { get; private set; } = string.Empty;

    public ClientType ClientType { get; private set; }

    public bool RequirePkce { get; private set; }

    public bool AllowRefreshTokens { get; private set; }

    public bool RequireConsent { get; private set; }

    public bool IsEnabled { get; private set; }

    public IReadOnlyCollection<string> RedirectUris => _redirectUris.AsReadOnly();

    public IReadOnlyCollection<string> AllowedScopes => _allowedScopes.AsReadOnly();

    public static RegisteredClient Create(
        Guid id,
        string clientId,
        string displayName,
        ClientType clientType,
        bool requirePkce,
        bool allowRefreshTokens,
        bool requireConsent,
        IEnumerable<string> redirectUris,
        IEnumerable<string> allowedScopes,
        DateTimeOffset createdAtUtc)
        => new(id, clientId, displayName, clientType, requirePkce, allowRefreshTokens, requireConsent, redirectUris, allowedScopes, createdAtUtc);

    public void UpdateDisplayName(string displayName, DateTimeOffset updatedAtUtc)
    {
        DisplayName = NormalizeDisplayName(displayName);
        Touch(updatedAtUtc);
    }

    public void UpdateConsentRequirement(bool requireConsent, DateTimeOffset updatedAtUtc)
    {
        RequireConsent = requireConsent;
        Touch(updatedAtUtc);
    }

    public void Disable(DateTimeOffset updatedAtUtc)
    {
        IsEnabled = false;
        Touch(updatedAtUtc);
    }

    public void Enable(DateTimeOffset updatedAtUtc)
    {
        IsEnabled = true;
        Touch(updatedAtUtc);
    }

    private static string NormalizeIdentifier(string value, string paramName)
    {
        var normalized = Guard.AgainstNullOrWhiteSpace(value, paramName);

        if (normalized.Length > 100)
        {
            throw new ArgumentOutOfRangeException(paramName, "Identifier cannot exceed 100 characters.");
        }

        return normalized;
    }

    private static string NormalizeDisplayName(string value)
    {
        var normalized = Guard.AgainstNullOrWhiteSpace(value, nameof(value));

        if (normalized.Length > 200)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "Display name cannot exceed 200 characters.");
        }

        return normalized;
    }

    private static IReadOnlyCollection<string> NormalizeRedirectUris(IEnumerable<string> redirectUris)
    {
        var normalized = Guard.AgainstNullOrEmpty(redirectUris, nameof(redirectUris))
            .Select(uri => Guard.AgainstNullOrWhiteSpace(uri, nameof(redirectUris)))
            .Select(uri =>
            {
                if (!Uri.TryCreate(uri, UriKind.Absolute, out var parsed) || string.IsNullOrWhiteSpace(parsed.Scheme) || string.IsNullOrWhiteSpace(parsed.Host))
                {
                    throw new ArgumentException($"Redirect URI '{uri}' must be an absolute URI.", nameof(redirectUris));
                }

                return parsed.ToString();
            })
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (normalized.Length == 0)
        {
            throw new ArgumentException("At least one redirect URI is required.", nameof(redirectUris));
        }

        return normalized;
    }

    private static IReadOnlyCollection<string> NormalizeScopes(IEnumerable<string> allowedScopes)
    {
        var normalized = Guard.AgainstNullOrEmpty(allowedScopes, nameof(allowedScopes))
            .Select(scope => Guard.AgainstNullOrWhiteSpace(scope, nameof(allowedScopes)).ToLowerInvariant())
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        if (normalized.Length == 0)
        {
            throw new ArgumentException("At least one scope is required.", nameof(allowedScopes));
        }

        return normalized;
    }
}