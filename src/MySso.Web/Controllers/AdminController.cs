using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySso.Application.Common.Interfaces;
using MySso.Application.Features.Clients;
using MySso.Application.Features.IdentityUsers;
using MySso.Application.Features.Roles;
using MySso.Application.Features.UserSessions;
using MySso.Contracts.Pagination;
using MySso.Domain.Enums;
using MySso.Web.ViewModels.Admin;

namespace MySso.Web.Controllers;

[Authorize(Roles = "Administrator")]
public sealed class AdminController : Controller
{
    private readonly IAdministrationQueryService _queryService;
    private readonly RegisterClientHandler _createClientHandler;
    private readonly CreateLocalUserHandler _createLocalUserHandler;
    private readonly CreateRoleHandler _createRoleHandler;
    private readonly RevokeUserSessionHandler _revokeUserSessionHandler;

    public AdminController(
        IAdministrationQueryService queryService,
        CreateLocalUserHandler createLocalUserHandler,
        CreateRoleHandler createRoleHandler,
        RegisterClientHandler createClientHandler,
        RevokeUserSessionHandler revokeUserSessionHandler)
    {
        _queryService = queryService;
        _createLocalUserHandler = createLocalUserHandler;
        _createRoleHandler = createRoleHandler;
        _createClientHandler = createClientHandler;
        _revokeUserSessionHandler = revokeUserSessionHandler;
    }

    [HttpGet("/admin")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var request = new PageRequest(1, 20);
        var users = await _queryService.GetUsersAsync(request, null, cancellationToken);
        var roles = await _queryService.GetRolesAsync(new PageRequest(1, 10), cancellationToken);
        var clients = await _queryService.GetClientsAsync(new PageRequest(1, 10), cancellationToken);
        var sessions = await _queryService.GetSessionsAsync(new PageRequest(1, 20), cancellationToken);
        var auditLogs = await _queryService.GetAuditLogsAsync(new PageRequest(1, 20), cancellationToken);

        ViewData["Users"] = users;
        ViewData["Roles"] = roles;
        ViewData["Clients"] = clients;
        ViewData["Sessions"] = sessions;
        ViewData["AuditLogs"] = auditLogs;

        return View();
    }

    [HttpGet("/admin/users/new")]
    public IActionResult CreateUser() => View(new CreateLocalUserViewModel());

    [HttpPost("/admin/users/new")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateUser(CreateLocalUserViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _createLocalUserHandler.HandleAsync(
            new CreateLocalUserCommand(model.Email, model.GivenName, model.FamilyName, model.Password, model.AssignAdministratorRole),
            cancellationToken);

        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Unable to create user.");
            return View(model);
        }

        TempData["AdminMessage"] = $"User {result.Value!.Email} created.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("/admin/roles/new")]
    public IActionResult CreateRole() => View(new CreateRoleViewModel());

    [HttpPost("/admin/roles/new")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateRole(CreateRoleViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _createRoleHandler.HandleAsync(new CreateRoleCommand(model.Name, model.Description), cancellationToken);

        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Unable to create role.");
            return View(model);
        }

        TempData["AdminMessage"] = $"Role {result.Value!.Name} created.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("/admin/clients/new")]
    public IActionResult CreateClient() => View(new CreateClientViewModel());

    [HttpPost("/admin/clients/new")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateClient(CreateClientViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var scopes = model.AllowedScopes
            .Split(new[] { ' ', ',', ';', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToArray();

        var result = await _createClientHandler.HandleAsync(
            new RegisterClientCommand(
                model.ClientId,
                model.DisplayName,
                model.ClientType,
                true,
                model.AllowRefreshTokens,
                model.RequireConsent,
                new[] { model.RedirectUri },
                scopes,
                string.IsNullOrWhiteSpace(model.ClientSecret) ? null : model.ClientSecret,
                string.IsNullOrWhiteSpace(model.PostLogoutRedirectUri) ? null : model.PostLogoutRedirectUri),
            cancellationToken);

        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Unable to register client.");
            return View(model);
        }

        TempData["AdminMessage"] = $"Client {result.Value!.ClientId} registered.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("/admin/sessions/{sessionId:guid}/revoke")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RevokeSession(Guid sessionId, CancellationToken cancellationToken)
    {
        var result = await _revokeUserSessionHandler.HandleAsync(new RevokeUserSessionCommand(sessionId, SessionRevocationReason.AdministratorForced), cancellationToken);
        TempData["AdminMessage"] = result.Succeeded ? "Session revoked." : result.ErrorMessage ?? "Unable to revoke session.";
        return RedirectToAction(nameof(Index));
    }
}