/*
 * IdentityServer Hosting Extensions
 * ================================
 * 
 * This file configures the Duende IdentityServer application that serves as the central
 * authentication and authorization server for the entire CleanCut application ecosystem.
 * 
 * AUTHENTICATION ARCHITECTURE ROLE:
 * ---------------------------------
 * This IdentityServer instance acts as the OAuth2/OpenID Connect provider that:
 * 
 * 1. ISSUES TOKENS - Creates JWT access tokens for API access and ID tokens for user identity
 * 2. VALIDATES CLIENTS - Authenticates client applications before issuing tokens
 * 3. MANAGES USERS - Handles user registration, login, logout using ASP.NET Core Identity
 * 4. PROVIDES DISCOVERY - Exposes metadata endpoints for clients to discover capabilities
 * 
 * INTEGRATION WITH CLIENT APPLICATIONS:
 * ------------------------------------
 * 
 * • CleanCut.API (Web API)
 *   ??? Validates tokens issued by this IdentityServer
 *   ??? Accepts tokens with audience claim "CleanCutAPI"
 *   ??? Uses JWT Bearer authentication middleware
 * 
 * • CleanCut.BlazorWebApp (Blazor Server)
 *   ??? Requests tokens using Client Credentials flow
 *   ??? Uses TokenService to get access tokens for API calls
 *   ??? Server-side authentication, no user interaction needed
 * 
 * • CleanCut.WebApp (MVC/Razor Pages)
 *   ??? Uses Authorization Code + PKCE flow for user authentication
 *   ??? Redirects users to this IdentityServer for login
 *   ??? Receives ID tokens for user identity and access tokens for API calls
 * 
 * • CleanCut.WinApp (WinForms - Future)
 *   ??? Would use Authorization Code + PKCE flow
 *   ??? Would launch system browser for authentication
 * 
 * SECURITY FEATURES IMPLEMENTED:
 * ------------------------------
 * • Enterprise password policies (12+ chars, complexity, uniqueness)
 * • Account lockout protection (5 failed attempts = 15min lockout)  
 * • Certificate-based JWT signing in production
 * • CORS restrictions to allowed client origins only
 * • HSTS headers for secure connections
 * • Comprehensive security event logging
 * • Secure cookie settings (HttpOnly, Secure, SameSite)
 * 
 * TOKEN ENDPOINTS:
 * ---------------
 * • /connect/token - Token endpoint for client credentials and auth code flows
 * • /connect/authorize - Authorization endpoint for user authentication
 * • /.well-known/openid_configuration - Discovery document
 * • /connect/userinfo - User info endpoint
 * • /connect/introspect - Token introspection endpoint
 * 
 * ENVIRONMENT BEHAVIOR:
 * --------------------
 * • Development: Uses developer signing credential, seeds test users
 * • Production: Requires certificate-based signing, secure configuration
 */

using CleanCut.Infranstructure.Identity.Data;
using CleanCut.Infranstructure.Identity.Models;
using Duende.IdentityServer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Security.Cryptography.X509Certificates;
using Duende.IdentityServer.Models;

namespace CleanCut.Infranstructure.Identity;

internal static class HostingExtensions
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddRazorPages();

        builder.Services.AddDbContext<ApplicationDbContext>(options =>
     options.UseSqlServer(
       builder.Configuration.GetConnectionString("DefaultConnection"),
 sqlOptions => sqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
     {
         // ? Enterprise password policy
         options.Password.RequiredLength = 12;
         options.Password.RequireDigit = true;
         options.Password.RequireUppercase = true;
         options.Password.RequireLowercase = true;
         options.Password.RequireNonAlphanumeric = true;
         options.Password.RequiredUniqueChars = 4;

         // ? Account lockout policy
         options.Lockout.AllowedForNewUsers = true;
         options.Lockout.MaxFailedAccessAttempts = 5;
         options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);

         // ? User requirements
         options.User.RequireUniqueEmail = true;
         options.SignIn.RequireConfirmedEmail = true;
     })
        .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        // ? Secure CORS configuration - restrict origins in production
        builder.Services.AddCors(options =>
         {
             options.AddPolicy("AllowTokenRequests", policy =>
                {
                    var allowedOrigins = builder.Environment.IsDevelopment()
                    ? new[] {
    "https://localhost:7142",
    "https://localhost:7297"
           }
               : builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

                    policy.WithOrigins(allowedOrigins)
               .AllowAnyMethod()
          .AllowAnyHeader()
         .AllowCredentials()
          .SetPreflightMaxAge(TimeSpan.FromMinutes(5));
                });
         });

        var identityServerBuilder = builder.Services
         .AddIdentityServer(options =>
            {
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;

                // see https://docs.duendesoftware.com/identityserver/v6/fundamentals/resources/
                options.EmitStaticAudienceClaim = true;

                // ? Enterprise security settings
                options.Authentication.CookieLifetime = TimeSpan.FromHours(2);
                options.Authentication.CookieSlidingExpiration = true;
                options.UserInteraction.LoginUrl = "/Account/Login";
                options.UserInteraction.LogoutUrl = "/Account/Logout";
            })
             .AddInMemoryIdentityResources(Config.IdentityResources)
                .AddInMemoryApiResources(Config.ApiResources) // ? Add API Resources
           .AddInMemoryApiScopes(Config.ApiScopes)
       .AddInMemoryClients(Config.Clients)
      .AddAspNetIdentity<ApplicationUser>();

        // ? Configure signing credential based on environment
        if (builder.Environment.IsDevelopment())
        {
            identityServerBuilder.AddDeveloperSigningCredential();
        }
        else
        {
            // ? Use certificate-based signing in production
            var certThumbprint = builder.Configuration["IdentityServer:SigningCertificateThumbprint"];
            if (!string.IsNullOrEmpty(certThumbprint))
            {
                var cert = LoadCertificateFromStore(certThumbprint);
                if (cert != null)
                {
                    identityServerBuilder.AddSigningCredential(cert);
                }
                else
                {
                    throw new InvalidOperationException($"Certificate with thumbprint {certThumbprint} not found");
                }
            }
            else
            {
                throw new InvalidOperationException("Signing certificate thumbprint not configured for production");
            }
        }

        // ? Enhanced authentication configuration
        // ?? Google authentication commented out for now
        /*
          builder.Services.AddAuthentication()
            .AddGoogle(options =>
       {
                  options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;

         // ? Load from secure configuration
            options.ClientId = builder.Configuration["Authentication:Google:ClientId"] ?? throw new InvalidOperationException("Google ClientId not configured");
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? throw new InvalidOperationException("Google ClientSecret not configured");
        });
      */

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

        return builder.Build();
    }

    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        app.UseSerilogRequestLogging();

        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            // ? Production security headers and error handling
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }

        // Static files first
        app.UseStaticFiles();

        // ? Force HTTPS redirect
        app.UseHttpsRedirection();

        // Enable CORS before routing
        app.UseCors("AllowTokenRequests");

        // Routing must be before IdentityServer
        app.UseRouting();

        // IdentityServer must be before Authorization
        app.UseIdentityServer();

        // Authorization comes after IdentityServer
        app.UseAuthorization();

        // ? Remove test endpoint in production
        if (app.Environment.IsDevelopment())
        {
            app.MapGet("/test-discovery", async context =>
             {
                 context.Response.ContentType = "application/json";
                 await context.Response.WriteAsync("{\n");
                 await context.Response.WriteAsync("  \"status\": \"IdentityServer is running\",\n");
                 await context.Response.WriteAsync($"  \"discovery_url\": \"{context.Request.Scheme}://{context.Request.Host}/.well-known/openid_configuration\",\n");
                 await context.Response.WriteAsync($"  \"token_url\": \"{context.Request.Scheme}://{context.Request.Host}/connect/token\"\n");
                 await context.Response.WriteAsync("}");
             });
        }

        app.MapRazorPages()
       .RequireAuthorization();

        return app;
    }

    // ? Certificate loading helper
    private static X509Certificate2? LoadCertificateFromStore(string thumbprint)
    {
        using var store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
        store.Open(OpenFlags.ReadOnly);
        var certificates = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false);
        return certificates.Count > 0 ? certificates[0] : null;
    }
}