using Duende.IdentityServer;
using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Mappers;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.OAuth.Claims;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.EntityFramework.Entities;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

var migrationsAssembly = typeof(Program).Assembly.GetName().Name;

// Add EF Core for IdentityServer configuration
builder.Services.AddDbContext<ConfigurationDbContext>(options =>
 options.UseSqlServer(builder.Configuration.GetConnectionString("IdentityServerDb")));

builder.Services.AddIdentityServer()
 .AddConfigurationStore(options =>
 {
     options.ConfigureDbContext = b => b.UseSqlServer(
     builder.Configuration.GetConnectionString("IdentityServerDb"),
     sql => sql.MigrationsAssembly(migrationsAssembly));
 })
 .AddOperationalStore(options =>
 {
     options.ConfigureDbContext = b => b.UseSqlServer(
     builder.Configuration.GetConnectionString("IdentityServerDb"),
     sql => sql.MigrationsAssembly(migrationsAssembly));
 });

// Add GitHub OAuth
builder.Services.AddAuthentication()
 .AddOAuth("GitHub", options =>
 {
     options.ClientId = builder.Configuration["Authentication:GitHub:ClientId"] ?? "";
     options.ClientSecret = builder.Configuration["Authentication:GitHub:ClientSecret"] ?? "";
     options.CallbackPath = "/signin-github";
     options.AuthorizationEndpoint = "https://github.com/login/oauth/authorize";
     options.TokenEndpoint = "https://github.com/login/oauth/access_token";
     options.UserInformationEndpoint = "https://api.github.com/user";
     options.Scope.Add("user:email");
     options.SaveTokens = true;
     options.Events.OnCreatingTicket = async context =>
     {
         var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
         request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
         request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", context.AccessToken);
         var response = await context.Backchannel.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, context.HttpContext.RequestAborted);
         response.EnsureSuccessStatusCode();
         var user = System.Text.Json.JsonDocument.Parse(await response.Content.ReadAsStringAsync());
         var root = user.RootElement;
         var claims = context.Identity;
         if (claims != null)
         {
             if (root.TryGetProperty("login", out var login))
                 claims.AddClaim(new System.Security.Claims.Claim("urn:github:login", login.GetString() ?? ""));
             if (root.TryGetProperty("id", out var id))
                 claims.AddClaim(new System.Security.Claims.Claim("urn:github:id", id.GetRawText()));
             if (root.TryGetProperty("html_url", out var url))
                 claims.AddClaim(new System.Security.Claims.Claim("urn:github:url", url.GetString() ?? ""));
         }
     };
 });

var app = builder.Build();

app.UseIdentityServer();

// Seed IdentityServer client and API resource/scope for Blazor WebApp
using (var scope = app.Services.CreateScope())
{
 var configDb = scope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
 // Seed client
 if (!configDb.Clients.Any(c => c.ClientId == "CleanCutBlazorWebApp"))
 {
 var client = new Duende.IdentityServer.Models.Client
 {
 ClientId = "CleanCutBlazorWebApp",
 ClientName = "CleanCut Blazor WebApp",
 AllowedGrantTypes = Duende.IdentityServer.Models.GrantTypes.Code,
 RequirePkce = true,
 RequireClientSecret = false,
 RedirectUris = { "https://localhost:7297/signin-oidc" },
 PostLogoutRedirectUris = { "https://localhost:7297/signout-callback-oidc" },
 AllowedScopes = { "openid", "profile", "CleanCutAPI" },
 AllowAccessTokensViaBrowser = true,
 AlwaysIncludeUserClaimsInIdToken = true,
 AccessTokenLifetime =3600
 };
 configDb.Clients.Add(client.ToEntity());
 configDb.SaveChanges();
 }
 // Seed API resource
 if (!configDb.ApiResources.Any(r => r.Name == "CleanCutAPI"))
 {
 var apiResource = new Duende.IdentityServer.Models.ApiResource("CleanCutAPI", "CleanCut API")
 {
 Scopes = { "CleanCutAPI" }
 };
 configDb.ApiResources.Add(apiResource.ToEntity());
 configDb.SaveChanges();
 }
 // Seed API scope
 if (!configDb.ApiScopes.Any(s => s.Name == "CleanCutAPI"))
 {
 var apiScope = new Duende.IdentityServer.Models.ApiScope("CleanCutAPI", "Access to CleanCut API");
 configDb.ApiScopes.Add(apiScope.ToEntity());
 configDb.SaveChanges();
 }
}

app.Run();
