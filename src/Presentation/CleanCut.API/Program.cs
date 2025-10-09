using CleanCut.Application;
using CleanCut.Infrastructure.Data;
using CleanCut.Infrastructure.Data.Seeding;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddControllers();

// Add CORS services
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSwagger", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add OpenAPI services (.NET 10 built-in)
builder.Services.AddOpenApi();

// Add Application layer
builder.Services.AddApplication();

// Add Data Infrastructure layer
builder.Services.AddDataInfrastructure(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // Enable CORS in development
    app.UseCors("AllowSwagger");
    
    // Map OpenAPI endpoint
    app.MapOpenApi();
    
    // Add Swagger UI
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "CleanCut API v1");
        options.RoutePrefix = string.Empty;
    });
    
    // Seed the database in development
    using (var scope = app.Services.CreateScope())
    {
        await DatabaseSeeder.SeedAsync(scope.ServiceProvider);
    }
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
