using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CleanCut.WebApp.Controllers;

/// <summary>
/// Account controller for authentication operations
/// </summary>
public class AccountController : Controller
{
  private readonly ILogger<AccountController> _logger;

    public AccountController(ILogger<AccountController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Initiate login process - redirects to IdentityServer
    /// </summary>
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
     _logger.LogInformation("Login initiated. ReturnUrl: {ReturnUrl}", returnUrl);
        
        // Store the return URL
        var properties = new AuthenticationProperties
        {
  RedirectUri = returnUrl ?? Url.Action("Index", "Home")
   };

      return Challenge(properties, OpenIdConnectDefaults.AuthenticationScheme);
  }

    /// <summary>
    /// Logout process - clears authentication and redirects to IdentityServer logout
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
 _logger.LogInformation("Logout initiated for user: {User}", User.Identity?.Name ?? "Unknown");

        // Create proper logout properties with absolute redirect URI
     var redirectUri = Url.Action("Index", "Home", null, Request.Scheme, Request.Host.Value);
 var properties = new AuthenticationProperties
   {
            RedirectUri = redirectUri
  };

        _logger.LogInformation("Logout properties created. RedirectUri: {RedirectUri}", properties.RedirectUri);

        // Sign out from both schemes - this will redirect to IdentityServer for logout
     return SignOut(properties, CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme);
    }

    /// <summary>
    /// Access denied page
    /// </summary>
    [HttpGet]
    public IActionResult AccessDenied()
    {
      _logger.LogWarning("Access denied for user: {User}", User.Identity?.Name ?? "Anonymous");
return View();
    }

  /// <summary>
    /// Display authentication status for debugging
    /// </summary>
    [HttpGet]
 [Authorize]
    public async Task<IActionResult> Profile()
    {
    _logger.LogInformation("Profile accessed by user: {User}", User.Identity?.Name ?? "Unknown");

        var accessToken = await HttpContext.GetTokenAsync("access_token");
    var idToken = await HttpContext.GetTokenAsync("id_token");
    var refreshToken = await HttpContext.GetTokenAsync("refresh_token");

ViewBag.AccessToken = accessToken;
   ViewBag.IdToken = idToken;
      ViewBag.RefreshToken = refreshToken;

  return View();
    }
}