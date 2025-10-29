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

    public static IEnumerable<ApiScope> ApiScopes =>
    new ApiScope[]
        {
     new ApiScope("scope1"),
  new ApiScope("scope2"),
     new ApiScope("CleanCutAPI", "CleanCut API Access")
     };

    public static IEnumerable<Client> Clients =>
    new Client[]
    {
            // Machine-to-machine client
       new Client
{
    ClientId = "m2m.client",
         ClientName = "Client Credentials Client",
     AllowedGrantTypes = GrantTypes.ClientCredentials,
       ClientSecrets = { new Secret("511536EF-F270-4058-80CA-1C89C192F69A".Sha256()) },
      AllowedScopes = { "CleanCutAPI" } // Updated to include your API
    },

  // Interactive testing client (no PKCE needed)
         new Client
     {
      ClientId = "interactive",
         ClientSecrets = { new Secret("49C1A7E1-0C79-4A89-A3D6-A37998FB86B0".Sha256()) },
  AllowedGrantTypes = GrantTypes.Code,
   RequirePkce = false, // ✅ No PKCE for confidential client
       
          RedirectUris = { "https://localhost:44300/signin-oidc" },
    FrontChannelLogoutUri = "https://localhost:44300/signout-oidc",
      PostLogoutRedirectUris = { "https://localhost:44300/signout-callback-oidc" },

       AllowOfflineAccess = true,
   AllowedScopes = { "openid", "profile", "CleanCutAPI" } // Added CleanCutAPI
       },

        // Simple API testing client
  new Client
        {
ClientId = "api-testing",
    ClientName = "API Testing Client",
   ClientSecrets = { new Secret("testing-secret-123".Sha256()) },
         AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
     RequirePkce = false,
   AllowOfflineAccess = true,
                AllowedScopes = { "openid", "profile", "CleanCutAPI" }
    },
  
// ✅ FIXED: CleanCut Blazor Server App (Client Credentials for API access)
  new Client
         {
        ClientId = "CleanCutBlazorWebApp",
        ClientName = "CleanCut Blazor Server WebApp",
    ClientSecrets = { new Secret("BlazorServerSecret2024!".Sha256()) },

     // ✅ Use client credentials for API access (machine-to-machine)
         AllowedGrantTypes = GrantTypes.ClientCredentials,
 
          // ✅ Only need API scopes for client credentials flow
 AllowedScopes = { "CleanCutAPI" },
         
   // Optional: Allow offline access if needed for long-running operations
           AllowOfflineAccess = false // Not needed for client credentials
            },

// ✅ FIXED: TempBlazorApp Client - Now supports client credentials flow
  new Client
       {
ClientId = "TempBlazorApp",
ClientName = "Temp Blazor App",
         ClientSecrets = { new Secret("TempSecret2024!".Sha256()) },
  
       // ✅ FIXED: Use client credentials for API access (machine-to-machine)
         AllowedGrantTypes = GrantTypes.ClientCredentials,
 
   // ✅ Only need API scopes for client credentials flow
        AllowedScopes = { "CleanCutAPI" },
         
   // Optional: Allow offline access if needed for long-running operations
 AllowOfflineAccess = false // Not needed for client credentials
  },

      // ✅ NEW: CleanCut MVC WebApp Client for client credentials flow
        new Client
        {
   ClientId = "CleanCutWebApp",
            ClientName = "CleanCut MVC WebApp",
    ClientSecrets = { new Secret("WebAppSecret2024!".Sha256()) },

            // ✅ Use client credentials for API access (machine-to-machine)
            AllowedGrantTypes = GrantTypes.ClientCredentials,

  // ✅ Only need API scopes for client credentials flow
        AllowedScopes = { "CleanCutAPI" },

      // Optional: Allow offline access if needed for long-running operations
            AllowOfflineAccess = false // Not needed for client credentials
        }
        };
}
