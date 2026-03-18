using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MySso.Api.Controllers;

[ApiController]
[Route("api/profile")]
public sealed class ProfileController : ControllerBase
{
    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        return Ok(new
        {
            subject = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier),
            name = User.FindFirstValue("name") ?? User.Identity?.Name,
            email = User.FindFirstValue("email") ?? User.FindFirstValue(ClaimTypes.Email),
            claims = User.Claims.Select(claim => new
            {
                claim.Type,
                claim.Value
            })
        });
    }
}