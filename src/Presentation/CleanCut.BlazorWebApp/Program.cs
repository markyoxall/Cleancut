using CleanCut.BlazorWebApp.Components;
using CleanCut.BlazorWebApp.Services;
using CleanCut.BlazorWebApp.State;
using CleanCut.BlazorWebApp.Extensions;

namespace CleanCut.BlazorWebApp;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddRazorComponents()
  .AddInteractiveServerComponents();

        // Configure Blazor Server options for better error handling
        builder.Services.Configure<Microsoft.AspNetCore.Components.Server.CircuitOptions>(options =>
        {
if (builder.Environment.IsDevelopment())
     {
   options.DetailedErrors = true;
            }
        });

        // Add HttpClient for API calls
        builder.Services.AddHttpClient();
        builder.Services.AddHttpContextAccessor();

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

 var app = builder.Build();

        if (!app.Environment.IsDevelopment())
        {
  app.UseExceptionHandler("/Error");
      app.UseHsts();
        }

        app.UseHttpsRedirection();
      app.UseStaticFiles();
        app.UseRouting();
  app.UseAntiforgery();

        app.MapRazorComponents<Components.App>()
            .AddInteractiveServerRenderMode();

        app.Run();
    }
}
