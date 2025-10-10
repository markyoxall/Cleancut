using CleanCut.WebApp.Mappings;
using CleanCut.WebApp.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.Console()
    .WriteTo.File("logs/cleancut-webapp-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure Problem Details for better error handling
builder.Services.AddProblemDetails();

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(ViewModelMappingProfile));

// ?? Configure HttpClient for API calls (instead of direct database access)
// ?? Configure HttpClient with longer timeout to handle Redis delays
builder.Services.AddHttpClient<IUserApiService, UserApiService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30); // Increase timeout from default 100 seconds
});

builder.Services.AddHttpClient<IProductApiService, ProductApiService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30); // Increase timeout from default 100 seconds
});

// ?? Register API Services
builder.Services.AddScoped<IUserApiService, UserApiService>();
builder.Services.AddScoped<IProductApiService, ProductApiService>();

// Add health checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    // Enhanced error handling in development
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

// Add health check endpoint
app.MapHealthChecks("/health");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
