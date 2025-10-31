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

      // ✅ Blazor Server App with proper security
          new Client
  {
          ClientId = "CleanCutBlazorWebApp",
 ClientName = "CleanCut Blazor Server WebApp",
       ClientSecrets = { new Secret(GetClientSecret(configuration, "CleanCutBlazorWebApp").Sha256()) },

      // ✅ Use client credentials for server-to-server API calls
            AllowedGrantTypes = GrantTypes.ClientCredentials,
     
      // ✅ API access scope
                AllowedScopes = { "CleanCutAPI" },
        
         // ✅ Security settings
    AllowOfflineAccess = false,
     AccessTokenLifetime = 3600, // 1 hour
RefreshTokenUsage = TokenUsage.OneTimeOnly,
          RefreshTokenExpiration = TokenExpiration.Sliding
         },

            // ✅ Web App client for user authentication
new Client
            {
    ClientId = "CleanCutWebApp",
         ClientName = "CleanCut MVC WebApp",
     ClientSecrets = { new Secret(GetClientSecret(configuration, "CleanCutWebApp").Sha256()) },

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

 // ✅ Add Swagger UI client for testing
        new Client
{
    ClientId = "swagger-ui",
       ClientName = "Swagger UI",
           AllowedGrantTypes = GrantTypes.Implicit,
     AllowAccessTokensViaBrowser = true,
         RequireClientSecret = false,
           
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
        // ✅ Development-only fallback secrets
        return clientId switch
        {
       "m2m.client" => "511536EF-F270-4058-80CA-1C89C192F69A",
            "CleanCutBlazorWebApp" => "BlazorServerSecret2024!",
            "CleanCutWebApp" => "WebAppSecret2024!",
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
          "CleanCutWebApp" => new List<string> { "https://localhost:7144/signin-oidc" },
      _ => new List<string>()
        };
    }
    
    private static List<string> GetDefaultPostLogoutRedirectUris(string clientId)
    {
        return clientId switch
        {
    "CleanCutBlazorWebApp" => new List<string> { "https://localhost:7297/signout-callback-oidc", "http://localhost:5091/signout-callback-oidc" },
            "CleanCutWebApp" => new List<string> { "https://localhost:7144/signout-callback-oidc" },
    _ => new List<string>()
   };
    }
}
