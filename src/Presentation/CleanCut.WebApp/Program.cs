/*
 * CleanCut MVC Web Application Program Configuration
 * =================================================
 * 
 * This file configures the CleanCut MVC web application which serves as a user-facing
 * client application in the OAuth2/OpenID Connect authentication architecture. It 
 * demonstrates modern web app authentication with user login and API access.
 * 
 * ROLE IN AUTHENTICATION ARCHITECTURE:
 * ------------------------------------
 * This MVC app acts as a PUBLIC CLIENT that:
 * 
 * 1. USER AUTHENTICATION - Redirects users to IdentityServer for login
 * 2. RECEIVES TOKENS - Gets both ID tokens (user identity) and access tokens (API access)
 * 3. API INTEGRATION - Uses access tokens to call CleanCut.API on behalf of users
 * 4. SESSION MANAGEMENT - Maintains user authentication state across requests
 * 
 * OAUTH 2.1 AUTHENTICATION FLOWS:
 * -------------------------------
 * 
 * ? Authorization Code + PKCE Flow (OAuth 2.1 Compliant):
 * • User clicks "Login" ? Redirected to IdentityServer
 *   • User enters credentials ? IdentityServer validates
 *• IdentityServer redirects back with authorization code
 *   • App exchanges code + PKCE verifier for tokens (ID + Access)
 *   • User is logged in with API access capabilities
 * 
 * This follows OAuth 2.1 security best practices with PKCE for enhanced security!
 */

using CleanCut.WebApp.Mappings;
using CleanCut.WebApp.Services;
using CleanCut.Infrastructure.Data;
using FluentValidation;
using Serilog;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using CleanCut.Application; // Register application services (including AutoMapper)

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.Console()
    .WriteTo.File("logs/cleancut-webapp-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// ? CRITICAL: Add authentication services FIRST
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.LogoutPath = "/Account/Logout";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
  options.SlidingExpiration = true;
 options.Cookie.Name = "CleanCutWebApp.Auth";
    
    // ? Enterprise security configuration
    options.Cookie.HttpOnly = true;
 options.Cookie.SecurePolicy = builder.Environment.IsDevelopment() 
        ? CookieSecurePolicy.SameAsRequest 
        : CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
 options.Cookie.IsEssential = true;
    options.Cookie.MaxAge = null;
    
    // ? Enterprise logout handling
    options.Events = new CookieAuthenticationEvents
    {
        OnSigningOut = context =>
      {
            Log.Information("Cookie authentication signing out for user: {User}", 
   context.HttpContext.User.Identity?.Name ?? "Unknown");
            return Task.CompletedTask;
        }
    };
})
.AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
{
    options.Authority = builder.Configuration["IdentityServer:Authority"] ?? "https://localhost:5001";
    options.ClientId = builder.Configuration["IdentityServer:ClientId"] ?? "CleanCutWebApp";
    // ? NO CLIENT SECRET - Public client with PKCE only
    
    options.ResponseType = "code";
    options.UsePkce = true;
    options.SaveTokens = true; // ? Critical: Store tokens for API access
    options.GetClaimsFromUserInfoEndpoint = true;
 
    options.Scope.Clear();
    options.Scope.Add("openid");
    options.Scope.Add("profile");
    options.Scope.Add("CleanCutAPI"); // ? Required for API access
    
    options.TokenValidationParameters = new TokenValidationParameters
    {
   NameClaimType = "name",
        RoleClaimType = "role"
    };
    
    // ? Enterprise-grade event handling
    options.Events = new OpenIdConnectEvents
{
        OnAuthenticationFailed = context =>
{
 Log.Error("OpenID Connect authentication failed: {Error}", context.Exception?.Message);
   return Task.CompletedTask;
    },
      OnTokenValidated = context =>
    {
     Log.Information("Token validated for user: {User}", context.Principal?.Identity?.Name);
       return Task.CompletedTask;
 },
 OnRedirectToIdentityProviderForSignOut = context =>
 {
   Log.Information("Redirecting to IdentityServer for logout");
 return Task.CompletedTask;
},
 OnSignedOutCallbackRedirect = context =>
 {
     Log.Information("Signed out callback redirect");
      return Task.CompletedTask;
   },
      OnRemoteSignOut = context =>
     {
 Log.Information("Remote sign out initiated");
  return Task.CompletedTask;
     }
    };
});

// ? Add authorization
builder.Services.AddAuthorization();

// Add services to the container.
builder.Services.AddControllersWithViews();
// Enable Razor Pages so pages under /Pages are routable (asp-page links)
builder.Services.AddRazorPages();

// ? CRITICAL: Add IHttpContextAccessor for token services
builder.Services.AddHttpContextAccessor();

// Configure Problem Details for better error handling
builder.Services.AddProblemDetails();

// Register only the parts of the Application layer that the MVC client needs
// Do NOT register MediatR handlers here because many handlers depend on IUnitOfWork
// and the WebApp is a client that does not host the data infrastructure. Registering
// MediatR handlers would trigger DI validation failures at startup.
// Instead, register AutoMapper and FluentValidation from the Application assembly.
{
    var appAssembly = typeof(CleanCut.Application.DependencyInjection).Assembly;
    // Configure AutoMapper (same approach used in the Application layer)
    var mapperConfig = new AutoMapper.MapperConfiguration(cfg => cfg.AddMaps(new[] { appAssembly }));
    var mapper = mapperConfig.CreateMapper();
    builder.Services.AddSingleton<AutoMapper.IMapper>(mapper);
    builder.Services.AddSingleton(mapperConfig);

    // Register FluentValidation validators from Application assembly
    builder.Services.AddValidatorsFromAssembly(appAssembly);
}

// ? Configure authenticated API clients with user authentication
builder.Services.AddApiClients(builder.Configuration);

// Add health checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    // Enhanced error handling in development
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseRouting();

// ? CRITICAL: Add authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();


app.MapStaticAssets();

// Make Razor Pages available at their routes (e.g. /Contact, /FAQ)
app.MapRazorPages();

// Add health check endpoint
app.MapHealthChecks("/health");

// ? Remove conflicting authentication endpoints - let AccountController handle them
// The AccountController.Login and AccountController.Logout methods will handle these routes

app.MapControllerRoute(
    name: "default",
  pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
