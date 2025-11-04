using TempBlazorApp.Components;
using TempBlazorApp.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add Blazor Server components
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// OAuth2.1/OIDC Authentication with IdentityServer (.NET 9 optimized)
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
  options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
    
    // Session-only cookies (don't persist when browser closes)
    options.Cookie.IsEssential = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.MaxAge = null;
})
.AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
{
    options.Authority = builder.Configuration["IdentityServer:Authority"] ?? "https://localhost:5001";
    options.ClientId = builder.Configuration["IdentityServer:ClientId"] ?? "TempBlazorApp";
    
    // OAuth2.1 Authorization Code Flow with PKCE (Public Client)
  options.ResponseType = "code";
    options.UsePkce = true;
    options.SaveTokens = true;
    options.GetClaimsFromUserInfoEndpoint = true;
 
    options.Scope.Clear();
 options.Scope.Add("openid");
    options.Scope.Add("profile");
    options.Scope.Add("CleanCutAPI");
  
    options.TokenValidationParameters = new TokenValidationParameters
    {
        NameClaimType = "name",
        RoleClaimType = "role"
    };
});

builder.Services.AddAuthorization();

// .NET 9 Blazor Server Circuit Configuration
builder.Services.Configure<Microsoft.AspNetCore.Components.Server.CircuitOptions>(options =>
{
    options.DetailedErrors = builder.Environment.IsDevelopment();
    options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(3);
    options.DisconnectedCircuitMaxRetained = 100;
    options.JSInteropDefaultCallTimeout = TimeSpan.FromMinutes(1);
});

// Essential services
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();
builder.Services.AddScoped<IProductService, ProductService>();

var app = builder.Build();

// Middleware pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Authentication endpoints
app.MapGet("/Account/Login", (string? returnUrl) => 
    Results.Challenge(new Microsoft.AspNetCore.Authentication.AuthenticationProperties
    {
    RedirectUri = returnUrl ?? "/"
    }));

app.MapPost("/Account/Logout", () => 
    Results.SignOut(authenticationSchemes: new[] { 
        CookieAuthenticationDefaults.AuthenticationScheme, 
        OpenIdConnectDefaults.AuthenticationScheme 
    }));

app.Run();
