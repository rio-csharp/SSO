using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MySso.Infrastructure.Identity;
using MySso.Web.ViewModels.Account;

namespace MySso.Web.Controllers;

public sealed class AccountController : Controller
{
    private readonly SignInManager<SsoIdentityUser> _signInManager;

    public AccountController(SignInManager<SsoIdentityUser> signInManager)
    {
        _signInManager = signInManager;
    }

    [HttpGet("/account/login")]
    public IActionResult Login(string? returnUrl = null)
        => View(new LoginViewModel { ReturnUrl = returnUrl });

    [HttpPost("/account/login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);

        if (result.Succeeded)
        {
            if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }

            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        return View(model);
    }

    [HttpPost("/account/logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction(nameof(HomeController.Index), "Home");
    }
}