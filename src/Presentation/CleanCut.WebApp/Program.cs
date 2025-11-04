/*
 * CleanCut MVC Web Application Program Configuration
 * =================================================
 * 
 * This file configures the CleanCut MVC web application which serves as a user-facing
 * client application in the OAuth2/OpenID Connect authentication architecture. It 
 * demonstrates traditional web app authentication with user login and API access.
 * 
 * ROLE IN AUTHENTICATION ARCHITECTURE:
 * ------------------------------------
 * This MVC app acts as a CONFIDENTIAL CLIENT that:
 * 
 * 1. USER AUTHENTICATION - Redirects users to IdentityServer for login
 * 2. RECEIVES TOKENS - Gets both ID tokens (user identity) and access tokens (API access)
 * 3. API INTEGRATION - Uses access tokens to call CleanCut.API on behalf of users
 * 4. SESSION MANAGEMENT - Maintains user authentication state across requests
 * 
 * AUTHENTICATION FLOWS SUPPORTED:
 * -------------------------------
 * 
 * • Authorization Code + PKCE Flow (Primary):
 *   ? User clicks "Login" ? Redirected to IdentityServer
 *   ? User enters credentials ? IdentityServer validates
 *   ? IdentityServer redirects back with authorization code
 *   ? App exchanges code for tokens (ID + Access)
 *   ? User is logged in with API access capabilities
 * 
 * This is the CORRECT approach for user-facing applications in 2025!
 */

using CleanCut.WebApp.Mappings;
using CleanCut.WebApp.Services;
using Serilog;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

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
    options.Cookie.HttpOnly = true;
 options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
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
    
    // Enhanced error handling
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
        }
    };
});

// ? Add authorization
builder.Services.AddAuthorization();

// Add services to the container.
builder.Services.AddControllersWithViews();

// ? CRITICAL: Add IHttpContextAccessor for token services
builder.Services.AddHttpContextAccessor();

// Configure Problem Details for better error handling
builder.Services.AddProblemDetails();

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(ViewModelMappingProfile));

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

// Add health check endpoint
app.MapHealthChecks("/health");

// ? Add authentication endpoints
app.MapGet("/Account/Login", () => Results.Challenge(new Microsoft.AspNetCore.Authentication.AuthenticationProperties
{
    RedirectUri = "/"
}, authenticationSchemes: new[] { OpenIdConnectDefaults.AuthenticationScheme }));

app.MapPost("/Account/Logout", () => Results.SignOut(
    new Microsoft.AspNetCore.Authentication.AuthenticationProperties
    {
 RedirectUri = "/"
}, 
   authenticationSchemes: new[] { CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme }));

app.MapControllerRoute(
    name: "default",
  pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
