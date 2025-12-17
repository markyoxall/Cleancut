using CleanCut.BlazorSPA;
using CleanCut.BlazorSPA.Pages.Services;
using CleanCut.BlazorSPA.Pages.State;
using CleanCut.BlazorSPA.Pages.Weather.State;
using CleanCut.BlazorSPA.Pages.Validation;
using FluentValidation;
using Fluxor;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Register the in-memory customer state as a scoped service (depends on IJSRuntime)
builder.Services.AddScoped<ICustomerState, InMemoryCustomerState>();

builder.Services.AddScoped<IProductState, InMemoryProductState>();

builder.Services.AddScoped<ICountryState, InMemoryCountryState>();

// Register FluentValidation validator
builder.Services.AddScoped<IValidator<CleanCut.BlazorSPA.Pages.Models.SimpleCustomer>, CustomerValidator>();

builder.Services.AddScoped<IValidator<CleanCut.BlazorSPA.Pages.Models.SimpleProduct>, ProductValidator>();

// Register Country validator
builder.Services.AddScoped<IValidator<CleanCut.BlazorSPA.Pages.Models.SimpleCountry>, CountryValidator>();

// Register JS interop wrapper
builder.Services.AddScoped<IJsInteropService, JsInteropService>();

// Global debug settings (toggle ErrorBoundary behavior application-wide)
builder.Services.AddSingleton<DebugSettings>();
// API call tracker for demo (counts simulated remote calls)
// This line is unchanged, but included for context
builder.Services.AddSingleton<IApiCallTracker, ApiCallTrackerService>();

// Configure Fluxor - scan the current assembly for features/reducers
builder.Services.AddFluxor(options =>
{
    options.ScanAssemblies(typeof(Program).Assembly);
});

// Register selector service for weather
builder.Services.AddScoped<IWeatherSelectors, WeatherSelectorService>();

var host = builder.Build();

// Ensure Fluxor store is initialized before the app starts to avoid race conditions
var store = host.Services.GetRequiredService<IStore>();
await store.InitializeAsync();

await host.RunAsync();
