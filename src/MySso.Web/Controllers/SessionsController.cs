using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySso.Application.Common.Interfaces;
using MySso.Application.Features.UserSessions;
using MySso.Contracts.Pagination;
using MySso.Domain.Enums;

namespace MySso.Web.Controllers;

[Authorize]
public sealed class SessionsController : Controller
{
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IAdministrationQueryService _queryService;
    private readonly RevokeUserSessionHandler _revokeUserSessionHandler;

    public SessionsController(
        IAdministrationQueryService queryService,
        ICurrentUserContext currentUserContext,
        RevokeUserSessionHandler revokeUserSessionHandler)
    {
        _queryService = queryService;
        _currentUserContext = currentUserContext;
        _revokeUserSessionHandler = revokeUserSessionHandler;
    }

    [HttpGet("/sessions")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var sessions = await _queryService.GetSessionsForSubjectAsync(new PageRequest(1, 50), _currentUserContext.SubjectId, cancellationToken);
        return View(sessions);
    }

    [HttpPost("/sessions/{sessionId:guid}/revoke")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Revoke(Guid sessionId, CancellationToken cancellationToken)
    {
        var result = await _revokeUserSessionHandler.HandleAsync(new RevokeUserSessionCommand(sessionId, SessionRevocationReason.UserRequested), cancellationToken);

        if (!result.Succeeded)
        {
            TempData["SessionError"] = result.ErrorMessage ?? "Unable to revoke session.";
        }
        else
        {
            TempData["SessionMessage"] = "Session revoked.";
        }

        return RedirectToAction(nameof(Index));
    }
}