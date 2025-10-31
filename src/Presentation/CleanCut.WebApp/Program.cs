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
 * This MVC app acts as a PUBLIC CLIENT that:
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
 *   ??? User clicks "Login" ? Redirected to IdentityServer
 *   ??? User enters credentials ? IdentityServer validates
 *   ??? IdentityServer redirects back with authorization code
 *   ??? App exchanges code for tokens (ID + Access)
 *   ??? User is logged in with API access capabilities
 * 
 * • Client Credentials Flow (API Access):
 *   ??? Background API calls using app-level credentials
 *   ??? Used for system-level operations not tied to specific users
 *   ??? Configured in addition to user authentication
 * 
 * MVC-SPECIFIC AUTHENTICATION FEATURES:
 * ------------------------------------
 * 
 * • Cookie-based Sessions:
 *   ??? User identity stored in encrypted authentication cookies
 *   ??? Automatic session management across page requests
 *   ??? Secure cookie configuration (HttpOnly, Secure, SameSite)
 * 
 * • Controller Authorization:
 *   ??? [Authorize] attributes on controllers and actions
 *   ??? Role-based access control for different user types
 *   ??? Automatic redirects to login page for unauthenticated users
 * 
 * • View Integration:
 *   ??? User claims available in Razor views (@User.Identity.Name)
 *   ??? Conditional content based on user roles and authentication status
 *   ??? Anti-forgery tokens for CSRF protection
 * 
 * INTEGRATION WITH OTHER COMPONENTS:
 * ---------------------------------
 * 
 * • CleanCut.Infrastructure.Identity (IdentityServer):
 *   ??? Handles user authentication and authorization
 *   ??? Issues ID tokens for user identity
 *   ??? Issues access tokens for API access
 *   ??? Manages user sessions and logout
 * 
 * • CleanCut.API:
 *   ??? Receives authenticated API requests from this MVC app
 *   ??? Validates access tokens in Authorization header
 *   ??? Returns user-specific data based on token claims
 *   ??? Enforces role-based access control
 * 
 * • Client Applications Comparison:
 *   ??? CleanCut.BlazorWebApp: Server-side, Client Credentials only
 *   ??? CleanCut.WebApp (This): User authentication + API access
 *   ??? CleanCut.WinApp: Desktop app, similar flow to this MVC app
 * 
 * SECURITY FEATURES:
 * -----------------
 * • PKCE (Proof Key for Code Exchange) for public client security
 * • State parameter validation to prevent CSRF attacks
 * • Secure cookie configuration with proper flags
 * • HTTPS enforcement in production
 * • Anti-forgery token validation for forms
 * • Content Security Policy headers
 * • Session timeout and sliding expiration
 * • Automatic token refresh for API calls
 * 
 * USER AUTHENTICATION FLOW:
 * -------------------------
 * 1. User visits protected page ? Automatic redirect to /Account/Login
 * 2. MVC app redirects to IdentityServer with PKCE challenge
 * 3. IdentityServer shows login form (alice/bob test accounts)
 * 4. User authenticates ? IdentityServer redirects back with auth code
 * 5. MVC app exchanges code for tokens using PKCE verifier
 * 6. User is logged in with authentication cookie set
 * 7. Subsequent API calls use access token automatically
 * 8. User can access protected controllers and views
 * 
 * API ACCESS PATTERNS:
 * -------------------
 * • Server-side API calls from controllers using HttpClient
 * • Client-side JavaScript AJAX calls with anti-forgery tokens
 * • Automatic Bearer token injection via AuthenticatedHttpMessageHandler
 * • Error handling for API authentication failures
 * 
 * CONFIGURATION REQUIREMENTS:
 * ---------------------------
 * • Client ID: "CleanCutWebApp"
 * • Client Secret: Secure secret from configuration/Key Vault
 * • Redirect URIs: /signin-oidc, /signout-callback-oidc
 * • Scopes: openid, profile, CleanCutAPI
 * • Response Types: code (Authorization Code flow)
 * • PKCE: Required for security
 * 
 * DEVELOPMENT VS PRODUCTION:
 * -------------------------
 * • Development: Relaxed CORS, detailed error pages, test accounts
 * • Production: Strict CORS, generic error pages, real user accounts
 * • Both: Secure cookie settings, HTTPS enforcement, CSP headers
 */

using CleanCut.WebApp.Mappings;
using CleanCut.WebApp.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.Console()
    .WriteTo.File("logs/cleancut-webapp-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure Problem Details for better error handling
builder.Services.AddProblemDetails();

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(ViewModelMappingProfile));

// ? Configure authenticated API clients (like BlazorWebApp)
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

app.UseAuthorization();

app.MapStaticAssets();

// Add health check endpoint
app.MapHealthChecks("/health");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
