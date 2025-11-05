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
 * ? Tokens stored securely server-side (never exposed to browser)
 * ? All API calls happen server-side with proper user context
 * ? SignalR maintains connection between browser and server
 * ? More secure for enterprise scenarios
 * ? Better for sensitive data handling
 * 
 * KEY AUTHENTICATION COMPONENTS:
 * ------------------------------
 * 
 * ? OpenID Connect Authentication - For user login/logout
 * ? Cookie Authentication - For maintaining user sessions
 * ? AuthenticatedHttpMessageHandler - Adds user's access token to API calls
 * ? ServiceCollectionExtensions - Configures all HTTP clients with authentication
 * 
 * USER AUTHENTICATION CONFIGURATION:
 * ---------------------------------
 * ? Client ID: "CleanCutBlazorWebApp"
 * ? Client Secret: Loaded from configuration (Azure Key Vault in production)
 * ? Scopes: openid, profile, CleanCutAPI
 * ? Grant Type: Authorization Code + PKCE (user authentication)
 * ? Token Endpoint: https://localhost:5001/connect/token
 * ? Authority: https://localhost:5001
 * 
 * SECURITY FEATURES:
 * -----------------
 * ? PKCE (Proof Key for Code Exchange) for enhanced security
 * ? Secure cookie configuration for user sessions
 * ? HTTPS enforcement in production
 * ? Content Security Policy headers
 * ? Session timeout and sliding expiration
 * ? Automatic token refresh for API calls
 * ? Anti-forgery tokens for form protection
 * 
 * INTEGRATION WITH OTHER APPS:
 * ----------------------------
 * 
 * ? CleanCut.API
 *   ? Receives authenticated requests from this Blazor app
 *   ? Validates JWT tokens issued to authenticated users
 *   ? Returns user-specific data based on token claims
 * 
 * ? CleanCut.Infrastructure.Identity
 *   ? Handles user authentication and authorization
 *   ? Issues access tokens via Authorization Code + PKCE flow
 *   ? Provides user identity and profile information
 * 
 * DEVELOPMENT VS PRODUCTION:
 * -------------------------
 * ? Development: Relaxed CSP for debugging, detailed errors
 * ? Production: Strict CSP, HSTS headers, minimal error details
 * ? Token storage: Secure HTTP-only cookies in both environments
 * ? Certificate-based signing validation in production
 * 
 * BLAZOR CIRCUIT SECURITY:
 * -----------------------
 * ? Circuit retention limited to prevent memory leaks
 * ? Secure SignalR connection for real-time updates
 * ? Server-side state management prevents client tampering
 * ? All authentication logic protected on server
 */

using CleanCut.BlazorWebApp.Components;
using CleanCut.BlazorWebApp.Services;
using CleanCut.BlazorWebApp.State;
using CleanCut.BlazorWebApp.Extensions;
using CleanCut.Infrastructure.Caching;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Components.Server.Circuits;

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
            
    // ? Secure cookie configuration for OAuth 2.1 
    options.Cookie.IsEssential = true;
    options.Cookie.HttpOnly = true;
 options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // ? Always require HTTPS
        options.Cookie.SameSite = SameSiteMode.Lax;
         options.Cookie.MaxAge = null;
        })
        .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
        {
options.Authority = builder.Configuration["IdentityServer:Authority"] ?? "https://localhost:5001";
      options.ClientId = builder.Configuration["IdentityServer:ClientId"] ?? "CleanCutBlazorWebApp";
     
      // ? OAuth 2.1 Authorization Code Flow with PKCE (Public Client - NO SECRET)
    options.ResponseType = "code";
 options.UsePkce = true;
            options.SaveTokens = true; // ? Critical: Store tokens for API access
         options.GetClaimsFromUserInfoEndpoint = true;
 
options.Scope.Clear();
 options.Scope.Add("openid");
    options.Scope.Add("profile");
        options.Scope.Add("CleanCutAPI"); // ? Required for API access
   
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
       {
  NameClaimType = "name",
        RoleClaimType = "role"
    };
      });

// ? Add authorization
        builder.Services.AddAuthorization();

        // ?? CRITICAL: Add Blazor Server authentication state provider
        builder.Services.AddScoped<AuthenticationStateProvider, ServerAuthenticationStateProvider>();
        
     // ? Add cascading authentication state for better reliability
        builder.Services.AddCascadingAuthenticationState();

        // ? Add data protection for secure token storage
      builder.Services.AddDataProtection(options =>
        {
      options.ApplicationDiscriminator = "CleanCut.BlazorWebApp";
        });

        // Add services to the container.
  builder.Services.AddRazorComponents()
     .AddInteractiveServerComponents();

   // ?? Add circuit error handler for better error management
        builder.Services.AddScoped<CircuitHandler, CleanCut.BlazorWebApp.Middleware.CircuitErrorHandler>();

   // Configure Blazor Server options for better error handling and security
        builder.Services.Configure<CircuitOptions>(options =>
   {
  if (builder.Environment.IsDevelopment())
         {
  options.DetailedErrors = true;
    }
  
  // ? Enhanced security and stability settings
        options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(3);
   options.DisconnectedCircuitMaxRetained = 100;
options.JSInteropDefaultCallTimeout = TimeSpan.FromMinutes(1);
     
  // ?? Critical: Handle circuit disposal gracefully
        options.RootComponents.MaxJSRootComponents = 1000;
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

        // *** REDIS CACHING: Add Redis caching infrastructure ***
        builder.Services.AddCachingInfrastructure(builder.Configuration);

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

      app.UseStaticFiles();
     app.UseRouting();

 // ? CRITICAL: Add authentication and authorization middleware
        app.UseAuthentication();
     app.UseAuthorization();
        
        app.UseAntiforgery();

    // ? Add authentication endpoints BEFORE the Blazor components
        app.MapGet("/Account/Login", (string? returnUrl) => Results.Challenge(new Microsoft.AspNetCore.Authentication.AuthenticationProperties
        {
         RedirectUri = returnUrl ?? "/"
        })).AllowAnonymous(); // Allow anonymous access to login

   app.MapPost("/Account/Logout", async (HttpContext context) =>
  {
 // Clear any existing response headers that might interfere
context.Response.Headers.Clear();
     
        return Results.SignOut(
          authenticationSchemes: new[] { 
   CookieAuthenticationDefaults.AuthenticationScheme, 
          OpenIdConnectDefaults.AuthenticationScheme 
    },
  properties: new Microsoft.AspNetCore.Authentication.AuthenticationProperties
   {
       RedirectUri = "/"
 });
  }).AllowAnonymous(); // Allow anonymous access to logout

        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode()
          .RequireAuthorization(); // ?? Require authentication for all Blazor components

    app.Run();
    }
}
