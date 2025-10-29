using CleanCut.BlazorWebApp.Components;
using CleanCut.BlazorWebApp.Services;
using CleanCut.BlazorWebApp.State;
using CleanCut.BlazorWebApp.Extensions; // add extension namespace
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace CleanCut.BlazorWebApp;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddRazorComponents()
  .AddInteractiveServerComponents();

 // Register product api clients & adapter via extension (reads appsettings)
        builder.Services.AddProductApiClients(builder.Configuration);

  // Other services unchanged
        builder.Services.AddScoped<ICustomerApiService, CustomerApiService>();
        builder.Services.AddScoped<ICountryApiService, CountryApiService>();
  builder.Services.AddScoped<IUiStateService, UiStateService>();

        // Register feature state services
    builder.Services.AddScoped<ICustomersState, CustomeraState>();
        builder.Services.AddScoped<IProductsState, ProductsState>();
 builder.Services.AddScoped<ICountriesState, CountriesState>();

    // Add HttpContextAccessor for Blazor Server (even though we're using AuthenticationStateProvider now)
        builder.Services.AddHttpContextAccessor();

        // Leave a default HttpClient for other uses
      builder.Services.AddHttpClient();

        // Add OIDC authentication for IdentityServer - Simple Blazor Server configuration
        builder.Services.AddAuthentication(options =>
   {
options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
       options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
        })
 .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
  {
            options.Authority = "https://localhost:5001";
  options.ClientId = "CleanCutBlazorWebApp";
            options.ClientSecret = "BlazorServerSecret2024!"; // Confidential client secret
 options.ResponseType = "code";
            options.SaveTokens = true; // This is all we need for Blazor Server
  
         // Required scopes
         options.Scope.Clear();
         options.Scope.Add("openid");
            options.Scope.Add("profile");
   options.Scope.Add("CleanCutAPI");
   
            options.RequireHttpsMetadata = true;
         
       // No PKCE needed for confidential client
            options.UsePkce = false;
            
            // Simple event logging
            options.Events = new OpenIdConnectEvents
      {
    OnRedirectToIdentityProvider = context =>
                {
        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Redirecting to IdentityServer for authentication");
           return Task.CompletedTask;
           },
 OnTokenValidated = context =>
 {
   var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
      logger.LogInformation("Token validated for user: {User}", context.Principal?.Identity?.Name);
return Task.CompletedTask;
       }
            };
        });

        // Add authorization
   builder.Services.AddAuthorization();

    var app = builder.Build();

        if (!app.Environment.IsDevelopment())
        {
    app.UseExceptionHandler("/Error");
            app.UseHsts();
        }

   app.UseHttpsRedirection();
        app.UseStaticFiles();
   app.UseRouting();
        
 app.UseAuthentication();
        app.UseAuthorization();
        
        app.UseAntiforgery();

 // Add authentication endpoints
   app.MapGet("/Account/Login", async (HttpContext context) =>
        {
            await context.ChallengeAsync(OpenIdConnectDefaults.AuthenticationScheme);
        });

   app.MapGet("/Account/Logout", async (HttpContext context) =>
        {
       await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
       await context.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
        });

        app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
   // Remove RequireAuthorization for now to prevent authentication loops
   // You can add [Authorize] attributes to specific pages instead

        app.Run();
    }
}
