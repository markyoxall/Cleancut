using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CleanCut.BlazorWebApp.Controllers;

/// <summary>
/// Local account controller to start the OIDC challenge from the Blazor host
/// </summary>
public class AccountController : Controller
{
    private readonly ILogger<AccountController> _logger;

    public AccountController(ILogger<AccountController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        _logger.LogInformation("AccountController.Login called. ReturnUrl: {ReturnUrl}", returnUrl);

        var props = new AuthenticationProperties
        {
            RedirectUri = returnUrl ?? Url.Content("~/")
        };

        return Challenge(props, OpenIdConnectDefaults.AuthenticationScheme);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        _logger.LogInformation("AccountController.Logout called for user {User}", User?.Identity?.Name ?? "unknown");

        var redirectUri = Url.Content("~/");
        var props = new AuthenticationProperties { RedirectUri = redirectUri };
        return SignOut(props, CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme);
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        _logger.LogWarning("AccessDenied hit for user {User}", User?.Identity?.Name ?? "anonymous");
        return View();
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        _logger.LogInformation("Profile accessed by {User}", User?.Identity?.Name ?? "unknown");
        var accessToken = await HttpContext.GetTokenAsync("access_token");
        ViewBag.AccessToken = accessToken;
        return View();
    }
}
