/*
 * IdentityServer Configuration File
 * ================================
 * 
 * This file defines the OpenID Connect/OAuth2 configuration for the CleanCut application's
 * centralized authentication and authorization server using Duende IdentityServer.
 * 
 * ROLE IN AUTHENTICATION ARCHITECTURE:
 * ------------------------------------
 * This serves as the central authority for all authentication and authorization in the CleanCut
 * ecosystem. It manages:
 * 
 * 1. IDENTITY RESOURCES - Claims about the user (OpenID, Profile info)
 * 2. API RESOURCES - Protected APIs that can be accessed with tokens
 * 3. API SCOPES - Granular permissions within APIs  
 * 4. CLIENTS - Applications that can request tokens
 * 
 * CLIENT APPLICATIONS CONFIGURED:
 * ------------------------------
 * 
 * • m2m.client - Machine-to-machine client for automated API access
 *   └── Uses: Client Credentials flow
 *   └── Purpose: Server-to-server communication without user interaction
 * 
 * • CleanCutBlazorWebApp - Blazor Server application
 *   └── Uses: Authorization Code flow with PKCE for user authentication
 *   └── Purpose: Server-side Blazor app with user login and API access
 * 
 * • CleanCutWebApp - MVC/Razor Pages application  
 *   └── Uses: Authorization Code flow with PKCE
 *   └── Purpose: Traditional web app with user authentication + API access
 * 
 * • swagger-ui - Swagger UI for API testing
 *   └── Uses: Implicit flow
 *   └── Purpose: Developer testing and API documentation
 * 
 * TOKEN FLOW:
 * -----------
 * 1. Client requests token from IdentityServer (/connect/token)
 * 2. IdentityServer validates client credentials/user authentication
 * 3. Issues JWT token with appropriate audience claim (CleanCutAPI)
 * 4. Client uses token to access protected CleanCut.API endpoints
 * 5. API validates token against IdentityServer's public keys
 * 
 * SECURITY FEATURES:
 * -----------------
 * • Enterprise password policies (12+ chars, complexity requirements)
 * • Account lockout protection (5 attempts, 15min lockout)
 * • Secure client secret management (Azure Key Vault in production)
 * • Certificate-based token signing (production)
 * • PKCE for public clients
 * • Proper audience claims for API access
 */

using Duende.IdentityServer.Models;

namespace CleanCut.Infranstructure.Identity;

public static class Config
{
    public static IEnumerable<IdentityResource> IdentityResources =>
        new IdentityResource[]
   {
  new IdentityResources.OpenId(),
       new IdentityResources.Profile(),
   };

    // ✅ Add API Resources for proper audience claims
    public static IEnumerable<ApiResource> ApiResources =>
     new ApiResource[]
        {
 new ApiResource("CleanCutAPI", "CleanCut API")
    {
    Scopes = { "CleanCutAPI" },
  UserClaims = { "role", "email", "name" }
       }
        };

    public static IEnumerable<ApiScope> ApiScopes =>
     new ApiScope[]
  {
   // ✅ Production-ready API scopes with proper claims
        new ApiScope("CleanCutAPI", "CleanCut API Access", new[] { "role", "email", "name" })
      };

    public static IEnumerable<Client> Clients =>
    GetClientsFromConfiguration();

    // ✅ Load clients from configuration with fallbacks
    private static IEnumerable<Client> GetClientsFromConfiguration()
    {
        var configuration = GetConfiguration();

   return new Client[]
{
      // ✅ Machine-to-machine client
      new Client
         {
   ClientId = "m2m.client",
       ClientName = "Client Credentials Client",
   AllowedGrantTypes = GrantTypes.ClientCredentials,
      ClientSecrets = { new Secret(GetClientSecret(configuration, "m2m.client").Sha256()) },
  AllowedScopes = { "CleanCutAPI" },

    // ✅ Enterprise security settings
        AccessTokenLifetime = 3600, // 1 hour
    AllowOfflineAccess = false,
  RefreshTokenUsage = TokenUsage.OneTimeOnly,
RefreshTokenExpiration = TokenExpiration.Sliding
  },

      // ✅ NEW: Background Service - Machine-to-machine client for automated product export
      new Client
      {
  ClientId = "cleancut-background-service",
      ClientName = "CleanCut Background Service",
          AllowedGrantTypes = GrantTypes.ClientCredentials,
          ClientSecrets = { new Secret(GetClientSecret(configuration, "cleancut-background-service").Sha256()) },
    AllowedScopes = { "CleanCutAPI" },

     // ✅ Background service security settings
          AccessTokenLifetime = 3600, // 1 hour - sufficient for periodic exports
          AllowOfflineAccess = false, // No user context needed
          RefreshTokenUsage = TokenUsage.OneTimeOnly,
      RefreshTokenExpiration = TokenExpiration.Sliding,
  
          // ✅ Background service specific settings
    AllowAccessTokensViaBrowser = false, // Server-side only
          UpdateAccessTokenClaimsOnRefresh = false, // No user claims to update
        AlwaysSendClientClaims = true, // Include client claims in token
      ClientClaimsPrefix = "client_", // Prefix for client claims
       
  // Add custom claims for the background service
Claims = new List<ClientClaim>
          {
       new ClientClaim("service_type", "background_export"),
  new ClientClaim("service_version", "1.0")
          }
      },

   // ✅ Blazor Server App - PUBLIC CLIENT for user authentication
      new Client
  {
     ClientId = "CleanCutBlazorWebApp",
 ClientName = "CleanCut Blazor Server WebApp",
   
   // ✅ PUBLIC CLIENT - No client secret needed for OAuth 2.1 with PKCE
     RequireClientSecret = false,

  // ✅ Use Authorization Code flow for user authentication
  AllowedGrantTypes = GrantTypes.Code,
    
   // ✅ PKCE REQUIRED for OAuth 2.1 public clients (RFC 7636)
 RequirePkce = true,
 
   // ✅ Redirect configuration for user authentication
   RedirectUris = GetRedirectUris(configuration, "CleanCutBlazorWebApp"),
      PostLogoutRedirectUris = GetPostLogoutRedirectUris(configuration, "CleanCutBlazorWebApp"),
 
      // ✅ Scopes for user authentication and API access
  AllowedScopes = { "openid", "profile", "CleanCutAPI" },
 
         // ✅ Security settings for user authentication
    AllowOfflineAccess = true, // For refresh tokens
 AccessTokenLifetime = 3600, // 1 hour
RefreshTokenUsage = TokenUsage.OneTimeOnly,
RefreshTokenExpiration = TokenExpiration.Sliding,
   
   // ✅ Additional security for OAuth 2.1
  RequireConsent = false,
    AllowPlainTextPkce = false // Require S256 PKCE method (OAuth 2.1 requirement)
    },

   // ✅ MVC Web App - PUBLIC CLIENT for user authentication
new Client
      {
    ClientId = "CleanCutWebApp",
 ClientName = "CleanCut MVC WebApp",
   
     // ✅ NO CLIENT SECRET - Public client for user authentication
   RequireClientSecret = false,

    // ✅ Use Authorization Code flow for user authentication
     AllowedGrantTypes = GrantTypes.Code,
       RequirePkce = true,

// ✅ Redirect configuration
     RedirectUris = GetRedirectUris(configuration, "CleanCutWebApp"),
PostLogoutRedirectUris = GetPostLogoutRedirectUris(configuration, "CleanCutWebApp"),
      
    // ✅ Scopes for user authentication and API access
          AllowedScopes = { "openid", "profile", "CleanCutAPI" },

      // ✅ Security settings
   AllowOfflineAccess = true,
     AccessTokenLifetime = 3600,
RefreshTokenUsage = TokenUsage.OneTimeOnly,
     RefreshTokenExpiration = TokenExpiration.Sliding,
       
     // ✅ Additional security
       RequireConsent = false,
    AllowPlainTextPkce = false
},

// ✅ TempBlazorApp - PUBLIC CLIENT for user authentication
new Client
{
    ClientId = "TempBlazorApp",
    ClientName = "Temp Blazor App Demo",
  
  // ✅ NO CLIENT SECRET - Public client for user authentication
    RequireClientSecret = false,

    // ✅ Use Authorization Code flow for user authentication
    AllowedGrantTypes = GrantTypes.Code,
    RequirePkce = true,

 // ✅ Redirect configuration for TempBlazorApp
    RedirectUris = GetRedirectUris(configuration, "TempBlazorApp"),
    PostLogoutRedirectUris = GetPostLogoutRedirectUris(configuration, "TempBlazorApp"),
    
  // ✅ Scopes for user authentication and API access
    AllowedScopes = { "openid", "profile", "CleanCutAPI" },

    // ✅ Security settings
  AllowOfflineAccess = true,
    AccessTokenLifetime = 3600,
    RefreshTokenUsage = TokenUsage.OneTimeOnly,
    RefreshTokenExpiration = TokenExpiration.Sliding,
    
    // ✅ Additional security
  RequireConsent = false,
    AllowPlainTextPkce = false
},

// ✅ WinApp client for desktop application (when you implement it)
new Client
{
    ClientId = "CleanCutWinApp",
    ClientName = "CleanCut Windows Desktop App",
    
    // ✅ PUBLIC CLIENT - Cannot store secrets securely
  RequireClientSecret = false,
    AllowedGrantTypes = GrantTypes.Code,
    RequirePkce = true, // ✅ REQUIRED for desktop apps per RFC 8252
    
  // ✅ Desktop app redirect URIs
RedirectUris = GetRedirectUris(configuration, "CleanCutWinApp"),
    PostLogoutRedirectUris = GetPostLogoutRedirectUris(configuration, "CleanCutWinApp"),

    // ✅ Scopes for user authentication and API access
    AllowedScopes = { "openid", "profile", "CleanCutAPI" },
    
    // ✅ Security settings for desktop apps
  AllowOfflineAccess = true, // For refresh tokens
    AccessTokenLifetime = 3600,
    RefreshTokenUsage = TokenUsage.OneTimeOnly,
RefreshTokenExpiration = TokenExpiration.Sliding,
  
    // ✅ Desktop-specific security
    RequireConsent = false,
    AllowPlainTextPkce = false // Require S256 code challenge method
},

 // ✅ Add Swagger UI client for testing (Authorization Code + PKCE)
 new Client
 {
     ClientId = "swagger-ui",
     ClientName = "Swagger UI",
     AllowedGrantTypes = GrantTypes.Code,
     RequirePkce = true,
     RequireClientSecret = false,
     AllowAccessTokensViaBrowser = true,

     RedirectUris = { "https://localhost:7142/swagger/oauth2-redirect.html" },
     PostLogoutRedirectUris = { "https://localhost:7142/swagger/" },

     AllowedScopes = { "openid", "profile", "CleanCutAPI" },
     // ✅ CORS for Swagger
     AllowedCorsOrigins = { "https://localhost:7142" }
 }
     };
    }

    // ✅ Secure configuration loading methods
    private static string GetClientSecret(IConfiguration configuration, string clientId)
    {
        // ✅ Try to load from secure configuration first
        var secret = configuration[$"IdentityServer:Clients:{clientId}:Secret"];

        // ✅ Fallback to default secrets for development only
        if (string.IsNullOrEmpty(secret) && IsEnvironmentDevelopment(configuration))
        {
            secret = GetDevelopmentClientSecret(clientId);
        }

        if (string.IsNullOrEmpty(secret))
        {
            throw new InvalidOperationException($"Client secret for {clientId} not found in configuration. Please configure 'IdentityServer:Clients:{clientId}:Secret'");
        }

        return secret;
    }

    private static bool IsEnvironmentDevelopment(IConfiguration configuration)
    {
        var environment = configuration["ASPNETCORE_ENVIRONMENT"];
        return string.Equals(environment, "Development", StringComparison.OrdinalIgnoreCase);
    }

    private static string GetDevelopmentClientSecret(string clientId)
    {
     // ✅ Development-only fallback secrets for CONFIDENTIAL clients only
        return clientId switch
    {
  "m2m.client" => "511536EF-F270-4058-80CA-1C89C192F69A",
     "cleancut-background-service" => "BackgroundServiceSecret2024!",
   // ✅ OAuth 2.1 Public Clients - No secrets configured (use PKCE only)
   // CleanCutBlazorWebApp => Public Client (Authorization Code + PKCE)
   // CleanCutWebApp => Public Client (Authorization Code + PKCE)
   // TempBlazorApp => Public Client (Authorization Code + PKCE)
  // Note: CleanCutWinApp is a public client and doesn't need a secret
   _ => throw new InvalidOperationException($"No development secret configured for client {clientId}")
    };
    }

    private static List<string> GetRedirectUris(IConfiguration configuration, string clientId)
    {
        var uris = configuration.GetSection($"IdentityServer:Clients:{clientId}:RedirectUris").Get<List<string>>();
        return uris ?? GetDefaultRedirectUris(clientId);
    }

    private static List<string> GetPostLogoutRedirectUris(IConfiguration configuration, string clientId)
    {
        var uris = configuration.GetSection($"IdentityServer:Clients:{clientId}:PostLogoutRedirectUris").Get<List<string>>();
        return uris ?? GetDefaultPostLogoutRedirectUris(clientId);
    }

    private static IConfiguration GetConfiguration()
    {
        // ✅ Access configuration safely with proper fallbacks
        var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
      .AddJsonFile("appsettings.json", optional: false)
          .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true)
            .AddEnvironmentVariables()
     .Build();

        return configuration;
    }

    // ✅ Default configurations for development
    private static List<string> GetDefaultRedirectUris(string clientId)
    {
  return clientId switch
          {
 "CleanCutBlazorWebApp" => new List<string> { "https://localhost:7297/signin-oidc", "http://localhost:5091/signin-oidc" },
      "CleanCutWebApp" => new List<string> { "https://localhost:7286/signin-oidc", "http://localhost:5200/signin-oidc" },
   "TempBlazorApp" => new List<string> { "https://localhost:7068/signin-oidc", "http://localhost:5110/signin-oidc" },
"CleanCutWinApp" => new List<string> { "http://localhost:8080/", "cleancut://callback" },
          _ => new List<string>()
};
    }

    private static List<string> GetDefaultPostLogoutRedirectUris(string clientId)
    {
        return clientId switch
 {
          "CleanCutBlazorWebApp" => new List<string> { "https://localhost:7297/signout-callback-oidc", "http://localhost:5091/signout-callback-oidc" },
      "CleanCutWebApp" => new List<string> { "https://localhost:7286/signout-callback-oidc", "http://localhost:5200/signout-callback-oidc" },
          "TempBlazorApp" => new List<string> { "https://localhost:7068/signout-callback-oidc", "http://localhost:5110/signout-callback-oidc" },
            "CleanCutWinApp" => new List<string> { "cleancut://logged-out" },
 _ => new List<string>()
        };
    }
}
