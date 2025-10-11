using CleanCut.BlazorWebApp.Components;
using CleanCut.BlazorWebApp.Services;
using CleanCut.BlazorWebApp.State;

namespace CleanCut.BlazorWebApp;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();

        // Register API services
        builder.Services.AddScoped<IProductApiService, ProductApiService>();
        builder.Services.AddScoped<IUserApiService, UserApiService>();

        // Register state management services
        builder.Services.AddScoped<IAppStateService, AppStateService>();
        builder.Services.AddScoped<IStateContainer, StateContainer>();

        // Simple HttpClient for making direct API calls
        builder.Services.AddHttpClient();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
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
