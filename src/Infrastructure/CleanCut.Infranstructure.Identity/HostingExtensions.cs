using CleanCut.Infranstructure.Identity.Data;
using CleanCut.Infranstructure.Identity.Models;
using Duende.IdentityServer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

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

        builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

   builder.Services
       .AddIdentityServer(options =>
       {
    options.Events.RaiseErrorEvents = true;
    options.Events.RaiseInformationEvents = true;
     options.Events.RaiseFailureEvents = true;
   options.Events.RaiseSuccessEvents = true;

 // see https://docs.duendesoftware.com/identityserver/v6/fundamentals/resources/
     options.EmitStaticAudienceClaim = true;
        })
   .AddInMemoryIdentityResources(Config.IdentityResources)
 .AddInMemoryApiScopes(Config.ApiScopes)
   .AddInMemoryClients(Config.Clients)
 .AddAspNetIdentity<ApplicationUser>()
     .AddDeveloperSigningCredential(); // Add signing credential for development

        // Authentication configuration - Google provider commented out for now
        builder.Services.AddAuthentication()
    .AddGoogle(options =>
      {
    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;

  // register your IdentityServer with Google at https://console.developers.google.com
  // enable the Google+ API
     // set the redirect URI to https://localhost:5001/signin-google
options.ClientId = "copy client ID from Google here";
    options.ClientSecret = "copy client secret from Google here";
  });
    return builder.Build();
    }

    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        app.UseSerilogRequestLogging();

  if (app.Environment.IsDevelopment())
     {
  app.UseDeveloperExceptionPage();
 }

    // Static files first
        app.UseStaticFiles();

 // Routing must be before IdentityServer
  app.UseRouting();

   // IdentityServer must be before Authorization
  app.UseIdentityServer();

 // Authorization comes after IdentityServer
   app.UseAuthorization();

     // Add a simple test endpoint to verify IdentityServer is working
      app.MapGet("/test-discovery", async context =>
  {
       context.Response.ContentType = "application/json";
  await context.Response.WriteAsync("{\n");
await context.Response.WriteAsync("  \"status\": \"IdentityServer is running\",\n");
       await context.Response.WriteAsync($"  \"discovery_url\": \"{context.Request.Scheme}://{context.Request.Host}/.well-known/openid_configuration\",\n");
      await context.Response.WriteAsync($"  \"token_url\": \"{context.Request.Scheme}://{context.Request.Host}/connect/token\"\n");
     await context.Response.WriteAsync("}");
        });

        app.MapRazorPages()
 .RequireAuthorization();

    return app;
    }
}