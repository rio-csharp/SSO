using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySso.Application.Common.Interfaces;

namespace MySso.Web.Controllers;

[Authorize]
public sealed class ProfileController : Controller
{
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IAdministrationQueryService _queryService;

    public ProfileController(IAdministrationQueryService queryService, ICurrentUserContext currentUserContext)
    {
        _queryService = queryService;
        _currentUserContext = currentUserContext;
    }

    [HttpGet("/profile")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var profile = await _queryService.GetCurrentUserProfileAsync(_currentUserContext.SubjectId, cancellationToken);

        if (profile is null)
        {
            return NotFound();
        }

        return View(profile);
    }
}