/*
 * Blazor Server Application Program Configuration
 * ==============================================
 *
 * Modernized to use .NET 10 Razor Components interactive hosting instead of a
 * fallback host Razor page (_Host.cshtml). This removes the dependency on the
 * host page and uses `MapRazorComponents` to render the root component.
 */

using CleanCut.BlazorWebApp.Extensions;
using CleanCut.BlazorWebApp.Components;
using Blazorise;
using CleanCut.Infrastructure.Caching;
using CleanCut.Infrastructure.Shared;
using Blazorise.Bootstrap5;
using Blazorise.Icons.FontAwesome;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Use the modern Razor Components hosting model  
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddServerSideBlazor()
    .AddCircuitOptions(options => options.DetailedErrors = true);

// Make authorization services available to Blazor components
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();  // REQUIRED in .NET 10

// Note: don't register ServerAuthenticationStateProvider explicitly here — the
// Blazor server hosting already provides an appropriate AuthenticationStateProvider
// when authentication is enabled and `CascadingAuthenticationState` is used in the app.

// Blazorise (Bootstrap 5 provider) + FontAwesome icons
builder.Services.AddBlazorise()
    .AddBootstrap5Providers()
    .AddFontAwesomeIcons();


// Register API clients and application services
builder.Services.AddProductApiClients(builder.Configuration);

// Make HttpContextAccessor available for cart keying
builder.Services.AddHttpContextAccessor();

// Configure caching infrastructure (use Redis when configured)
try
{
    builder.Services.AddCachingInfrastructure(builder.Configuration);
}
catch
{
    // Fail-safe: if caching infrastructure registration fails, keep going with defaults
}

// Ensure antiforgery services are available for MVC/Razor endpoints and any
// component-level antiforgery usage. We don't add a custom middleware here;
// registering the service is sufficient for filters and helpers that require
// IAntiforgery. The header name is useful for AJAX or fetch-style requests.
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.HttpOnly = true;
});

// Add MVC controllers so we can expose /Account endpoints in this host
builder.Services.AddControllersWithViews();

// Authentication configuration - keep minimal and use configuration if present
var authority = builder.Configuration["IdentityServer:Authority"] ?? "https://localhost:5001";
var clientId = builder.Configuration["IdentityServer:ClientId"] ?? "CleanCutBlazorWebApp";

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.Cookie.HttpOnly = true;
    // Allow cross-site redirects for the OIDC sign-in flow. Require Secure for modern browsers.
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
})
.AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
{
    options.Authority = authority;
    options.ClientId = clientId;
    options.ResponseType = "code";
    options.UsePkce = true;
    options.SaveTokens = true;
    options.GetClaimsFromUserInfoEndpoint = true;
    options.Scope.Clear();
    options.Scope.Add("openid");
    options.Scope.Add("profile");
    options.Scope.Add("CleanCutAPI");
    // Make OIDC correlation/nonce cookies compatible with cross-site flows and secure only
    options.NonceCookie.SameSite = SameSiteMode.None;
    options.NonceCookie.SecurePolicy = CookieSecurePolicy.Always;
    options.CorrelationCookie.SameSite = SameSiteMode.None;
    options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        NameClaimType = "name",
        RoleClaimType = "role"
    };
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Development-only: respond to BrowserLink requests with a small placeholder
// This prevents console errors when the BrowserLink script is present in the client
// but Visual Studio/IDE BrowserLink middleware is not active in the host.
if (app.Environment.IsDevelopment())
{
    // Enable BrowserLink middleware during development for IDE integration and live reload support
    app.UseBrowserLink();
}

// Enable authentication & authorization
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Antiforgery middleware required for endpoints that carry antiforgery metadata
// (MVC/Razor Pages/Identity endpoints). Place after authentication/authorization
// and before endpoint mapping so ValidateAntiForgeryToken and similar metadata
// are honored at runtime.
app.UseAntiforgery();

// Note: Blazorise requires JS initialization on the client. We include Blazorise static files in App.razor and call initialization from the client.

// Map controller routes (conventional) so /Account/Login works
app.MapDefaultControllerRoute();

// Map modern Razor Components root component and enable server render mode
// (prerendering set on component)
// Register static asset mapping so `Assets[...]` helper works in App.razor
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
