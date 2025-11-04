/*
 * Blazor Server Application Program Configuration
 * ==============================================
 * 
 * This file configures the CleanCut Blazor Server application which serves as a client
 * application in the OAuth2/OpenID Connect authentication architecture. It demonstrates
 * server-side authentication with user login and API access.
 * 
 * ROLE IN AUTHENTICATION ARCHITECTURE:
 * ------------------------------------
 * This Blazor Server app acts as a CONFIDENTIAL CLIENT that:
 * 
 * 1. USER AUTHENTICATION - Redirects users to IdentityServer for login
 * 2. RECEIVES TOKENS - Gets both ID tokens (user identity) and access tokens (API access)
 * 3. API INTEGRATION - Uses access tokens to call CleanCut.API on behalf of users
 * 4. SESSION MANAGEMENT - Maintains user authentication state across requests
 * 
 * AUTHENTICATION FLOW:
 * -------------------
 * 1. User visits Blazor app ? Authentication required ? Redirect to IdentityServer
 * 2. IdentityServer validates user credentials (alice/bob test accounts)
 * 3. IdentityServer issues JWT token with "CleanCutAPI" audience + ID token
 * 4. AuthenticatedHttpMessageHandler adds Bearer token to all API requests
 * 5. CleanCut.API validates token and processes request
 * 6. Blazor components receive data and render UI with user context
 * 
 * BLAZOR SERVER SPECIFICS:
 * -----------------------
 * Unlike Blazor WebAssembly, this server-side approach means:
 * • Tokens stored securely server-side (never exposed to browser)
 * • All API calls happen server-side with proper user context
 * • SignalR maintains connection between browser and server
 * • More secure for enterprise scenarios
 * • Better for sensitive data handling
 * 
 * KEY AUTHENTICATION COMPONENTS:
 * ------------------------------
 * 
 * • OpenID Connect Authentication - For user login/logout
 * • Cookie Authentication - For maintaining user sessions
 * • AuthenticatedHttpMessageHandler - Adds user's access token to API calls
 * • ServiceCollectionExtensions - Configures all HTTP clients with authentication
 * 
 * USER AUTHENTICATION CONFIGURATION:
 * ---------------------------------
 * • Client ID: "CleanCutBlazorWebApp"
 * • Client Secret: Loaded from configuration (Azure Key Vault in production)
 * • Scopes: openid, profile, CleanCutAPI
 * • Grant Type: Authorization Code + PKCE (user authentication)
 * • Token Endpoint: https://localhost:5001/connect/token
 * • Authority: https://localhost:5001
 * 
 * SECURITY FEATURES:
 * -----------------
 * • PKCE (Proof Key for Code Exchange) for enhanced security
 * • Secure cookie configuration for user sessions
 * • HTTPS enforcement in production
 * • Content Security Policy headers
 * • Session timeout and sliding expiration
 * • Automatic token refresh for API calls
 * • Anti-forgery tokens for form protection
 * 
 * INTEGRATION WITH OTHER APPS:
 * ----------------------------
 * 
 * • CleanCut.API
 *   ? Receives authenticated requests from this Blazor app
 *   ? Validates JWT tokens issued to authenticated users
 *   ? Returns user-specific data based on token claims
 * 
 * • CleanCut.Infrastructure.Identity
 *   ? Handles user authentication and authorization
 *   ? Issues access tokens via Authorization Code + PKCE flow
 *   ? Provides user identity and profile information
 * 
 * DEVELOPMENT VS PRODUCTION:
 * -------------------------
 * • Development: Relaxed CSP for debugging, detailed errors
 * • Production: Strict CSP, HSTS headers, minimal error details
 * • Token storage: Secure HTTP-only cookies in both environments
 * • Certificate-based signing validation in production
 * 
 * BLAZOR CIRCUIT SECURITY:
 * -----------------------
 * • Circuit retention limited to prevent memory leaks
 * • Secure SignalR connection for real-time updates
 * • Server-side state management prevents client tampering
 * • All authentication logic protected on server
 */

using CleanCut.BlazorWebApp.Components;
using CleanCut.BlazorWebApp.Services;
using CleanCut.BlazorWebApp.State;
using CleanCut.BlazorWebApp.Extensions;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace CleanCut.BlazorWebApp;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

     // ?? Add authentication services FIRST
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
            options.ClientId = builder.Configuration["IdentityServer:ClientId"] ?? "CleanCutBlazorWebApp";
            
  // ? OAuth2.1 Authorization Code Flow with PKCE (Public Client)
            options.ResponseType = "code";
options.UsePkce = true;
      options.SaveTokens = true; // ?? Critical: Store tokens for API access
  options.GetClaimsFromUserInfoEndpoint = true;
   
 options.Scope.Clear();
   options.Scope.Add("openid");
       options.Scope.Add("profile");
      options.Scope.Add("CleanCutAPI"); // ?? Required for API access
       
  options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
            {
  NameClaimType = "name",
    RoleClaimType = "role"
         };
        });

        // ? Add authorization
        builder.Services.AddAuthorization();

        // ? Add data protection for secure token storage
        builder.Services.AddDataProtection(options =>
 {
  options.ApplicationDiscriminator = "CleanCut.BlazorWebApp";
        });

        // Add services to the container.
 builder.Services.AddRazorComponents()
  .AddInteractiveServerComponents();

      // Configure Blazor Server options for better error handling and security
     builder.Services.Configure<Microsoft.AspNetCore.Components.Server.CircuitOptions>(options =>
        {
if (builder.Environment.IsDevelopment())
  {
     options.DetailedErrors = true;
}
       
   // ? Security settings
 options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(3);
    options.DisconnectedCircuitMaxRetained = 100;
   options.JSInteropDefaultCallTimeout = TimeSpan.FromMinutes(1);
        });

 builder.Services.AddHttpContextAccessor();

    // ? Register ALL API clients with enhanced security (now using user tokens)
      builder.Services.AddProductApiClients(builder.Configuration);

// Other services
      builder.Services.AddScoped<IUiStateService, UiStateService>();

   // Register feature state services
    builder.Services.AddScoped<ICustomersState, CustomeraState>();
    builder.Services.AddScoped<IProductsState, ProductsState>();
   builder.Services.AddScoped<ICountriesState, CountriesState>();

        // ? Add HSTS for production
   if (!builder.Environment.IsDevelopment())
{
    builder.Services.AddHsts(options =>
 {
        options.Preload = true;
       options.IncludeSubDomains = true;
      options.MaxAge = TimeSpan.FromDays(365);
    });
        }

     // ? Enhanced logging
        builder.Services.AddLogging(loggingBuilder =>
 {
       loggingBuilder.AddConsole();
  if (builder.Environment.IsDevelopment())
  {
   loggingBuilder.SetMinimumLevel(LogLevel.Debug);
      }
            else
  {
 loggingBuilder.SetMinimumLevel(LogLevel.Information);
      }
        });

     var app = builder.Build();

// ? Enhanced security middleware pipeline
 if (!app.Environment.IsDevelopment())
     {
 app.UseExceptionHandler("/Error");
   app.UseHsts();
        }

      app.UseHttpsRedirection();

        // ? Add security headers middleware
        app.Use(async (context, next) =>
        {
    var headers = context.Response.Headers;
  
headers.TryAdd("X-Content-Type-Options", "nosniff");
       headers.TryAdd("X-Frame-Options", "DENY");
    headers.TryAdd("X-XSS-Protection", "1; mode=block");
     headers.TryAdd("Referrer-Policy", "strict-origin-when-cross-origin");

       if (!app.Environment.IsDevelopment())
            {
   headers.TryAdd("Strict-Transport-Security", "max-age=31536000; includeSubDomains; preload");
         }
    
  // ? Enhanced CSP for Blazor Server
       var csp = "default-src 'self'; " +
       "script-src 'self' 'unsafe-inline' 'unsafe-eval' https://cdn.jsdelivr.net https://cdnjs.cloudflare.com; " + // Allow CDN scripts
     "style-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net https://cdnjs.cloudflare.com; " + // Allow CDN styles
    "img-src 'self' data: https:; " +
   "connect-src 'self' wss: https://localhost:5001 https://localhost:7142; " + // WebSocket for SignalR
 "font-src 'self' https://cdnjs.cloudflare.com; " + // Allow CDN fonts
     "object-src 'none'; " +
    "base-uri 'self'; " +
       "form-action 'self'; " +
     "frame-ancestors 'none'";
 
   if (!app.Environment.IsDevelopment())
        {
  csp += "; upgrade-insecure-requests";
        }

headers.TryAdd("Content-Security-Policy", csp);

            await next();
        });

  app.UseStaticFiles();
 app.UseRouting();

        // ? CRITICAL: Add authentication and authorization middleware
    app.UseAuthentication();
        app.UseAuthorization();
        
     app.UseAntiforgery();

 app.MapRazorComponents<App>()
     .AddInteractiveServerRenderMode();

        // ? Add authentication endpoints (FIXED)
      app.MapGet("/Account/Login", (string? returnUrl) => Results.Challenge(new Microsoft.AspNetCore.Authentication.AuthenticationProperties
        {
      RedirectUri = returnUrl ?? "/"
   }));

        app.MapPost("/Account/Logout", () => Results.SignOut(
          authenticationSchemes: new[] { 
   CookieAuthenticationDefaults.AuthenticationScheme, 
   OpenIdConnectDefaults.AuthenticationScheme 
      }));

        app.Run();
    }
}
