using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySso.Application.Common.Interfaces;
using MySso.Contracts.Pagination;

namespace MySso.Web.Controllers;

[Authorize(Roles = "Administrator")]
public sealed class AdminController : Controller
{
    private readonly IAdministrationQueryService _queryService;

    public AdminController(IAdministrationQueryService queryService)
    {
        _queryService = queryService;
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
}