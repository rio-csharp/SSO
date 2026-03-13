using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using MySso.Application.Common.Interfaces;

namespace MySso.Infrastructure.Services;

public sealed class HttpCurrentUserContext : ICurrentUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpCurrentUserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public bool IsAuthenticated => Principal.Identity?.IsAuthenticated ?? false;

    public string SubjectId =>
        Principal.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? Principal.FindFirstValue("sub")
        ?? string.Empty;

    public string? DisplayName => Principal.Identity?.Name;

    public string? IpAddress => _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();

    public bool IsInRole(string roleName) => Principal.IsInRole(roleName);

    private ClaimsPrincipal Principal => _httpContextAccessor.HttpContext?.User ?? new ClaimsPrincipal(new ClaimsIdentity());
}