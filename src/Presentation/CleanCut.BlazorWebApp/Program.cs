using CleanCut.BlazorWebApp.Components;
using CleanCut.BlazorWebApp.Services;
using CleanCut.BlazorWebApp.State;
using CleanCut.BlazorWebApp.Extensions; // add extension namespace

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

        var app = builder.Build();

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseAntiforgery();
        app.MapStaticAssets();
        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        app.Run();
    }
}
