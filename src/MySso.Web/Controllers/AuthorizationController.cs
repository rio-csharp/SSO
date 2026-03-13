using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MySso.Infrastructure.Identity;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace MySso.Web.Controllers;

public sealed class AuthorizationController : Controller
{
    private readonly SignInManager<SsoIdentityUser> _signInManager;
    private readonly UserManager<SsoIdentityUser> _userManager;

    public AuthorizationController(SignInManager<SsoIdentityUser> signInManager, UserManager<SsoIdentityUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [HttpGet("~/connect/authorize")]
    [HttpPost("~/connect/authorize")]
    public async Task<IActionResult> Authorize()
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return Challenge(new AuthenticationProperties
            {
                RedirectUri = Request.PathBase + Request.Path + Request.QueryString
            }, IdentityConstants.ApplicationScheme);
        }

        var user = await _userManager.GetUserAsync(User);

        if (user is null || !user.IsActive)
        {
            await _signInManager.SignOutAsync();

            return Challenge(new AuthenticationProperties
            {
                RedirectUri = Request.PathBase + Request.Path + Request.QueryString
            }, IdentityConstants.ApplicationScheme);
        }

        var identity = new ClaimsIdentity(TokenValidationParameters.DefaultAuthenticationType, Claims.Name, Claims.Role);
        identity.AddClaim(new Claim(Claims.Subject, user.Id.ToString()));
        identity.AddClaim(new Claim(Claims.Email, user.Email ?? string.Empty));
        identity.AddClaim(new Claim(Claims.Name, user.UserName ?? user.Email ?? user.Id.ToString()));
        identity.AddClaim(new Claim(Claims.PreferredUsername, user.UserName ?? user.Email ?? user.Id.ToString()));

        if (!string.IsNullOrWhiteSpace(user.GivenName))
        {
            identity.AddClaim(new Claim(Claims.GivenName, user.GivenName));
        }

        if (!string.IsNullOrWhiteSpace(user.FamilyName))
        {
            identity.AddClaim(new Claim(Claims.FamilyName, user.FamilyName));
        }

        foreach (var role in await _userManager.GetRolesAsync(user))
        {
            identity.AddClaim(new Claim(Claims.Role, role));
        }

        identity.SetScopes(GetRequestedScopes());
        identity.SetResources("resource_api");
        identity.SetDestinations(GetDestinations);

        return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    [HttpGet("~/connect/logout")]
    [HttpPost("~/connect/logout")]
    public async Task<IActionResult> Logout()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            await _signInManager.SignOutAsync();
        }

        return SignOut(new AuthenticationProperties
        {
            RedirectUri = Url.Action(nameof(HomeController.Index), "Home")
        }, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    [Authorize(AuthenticationSchemes = OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)]
    [HttpGet("~/connect/userinfo")]
    [HttpPost("~/connect/userinfo")]
    public IActionResult UserInfo()
    {
        return Ok(new
        {
            sub = User.FindFirst(Claims.Subject)?.Value,
            name = User.FindFirst(Claims.Name)?.Value,
            preferred_username = User.FindFirst(Claims.PreferredUsername)?.Value,
            email = User.FindFirst(Claims.Email)?.Value,
            given_name = User.FindFirst(Claims.GivenName)?.Value,
            family_name = User.FindFirst(Claims.FamilyName)?.Value,
            role = User.FindAll(Claims.Role).Select(claim => claim.Value).ToArray()
        });
    }

    private static IEnumerable<string> GetDestinations(Claim claim)
        => claim.Type switch
        {
            Claims.Name or Claims.Email or Claims.Subject or Claims.PreferredUsername or Claims.GivenName or Claims.FamilyName or Claims.Role
                => new[] { Destinations.AccessToken, Destinations.IdentityToken },
            _ => new[] { Destinations.AccessToken }
        };

    private IEnumerable<string> GetRequestedScopes()
    {
        var rawValue = Request.HasFormContentType
            ? Request.Form["scope"].ToString()
            : Request.Query["scope"].ToString();

        return string.IsNullOrWhiteSpace(rawValue)
            ? new[] { Scopes.OpenId, Scopes.Profile }
            : rawValue.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }
}