using CleanCut.BlazorWebApp.Components;
using CleanCut.BlazorWebApp.Services;
using CleanCut.BlazorWebApp.State;
using CleanCut.BlazorWebApp.Extensions; // add extension namespace
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

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
        builder.Services.AddScoped<IUserApiService, UserApiService>();
        builder.Services.AddScoped<ICountryApiService, CountryApiService>();
        builder.Services.AddScoped<IUiStateService, UiStateService>();

        // Register feature state services
        builder.Services.AddScoped<IUsersState, UsersState>();
        builder.Services.AddScoped<IProductsState, ProductsState>();
        builder.Services.AddScoped<ICountriesState, CountriesState>();

        // Leave a default HttpClient for other uses
        builder.Services.AddHttpClient();

        // Add OIDC authentication for IdentityServer
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultScheme = "Cookies";
            options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
        })
        .AddCookie("Cookies")
        .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
        {
            options.Authority = builder.Configuration["IdentityServer:Authority"] ?? "https://localhost:5003";
            options.ClientId = "CleanCutBlazorWebApp";
            options.ResponseType = "code";
            options.SaveTokens = true;
            options.Scope.Add("openid");
            options.Scope.Add("profile");
            options.Scope.Add("CleanCutAPI");
            options.RequireHttpsMetadata = true;
        });

        var app = builder.Build();

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseAntiforgery();
        app.MapRazorComponents<App>()
         .AddInteractiveServerRenderMode();
     

        app.Run();
    }
}
