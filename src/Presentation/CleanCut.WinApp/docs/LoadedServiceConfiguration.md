# Detailed Guide: ServiceConfiguration and Application Startup in CleanCut.WinApp

---

# Introduction and Purpose

This document explains the `ServiceConfiguration` responsibilities and the startup sequence used by CleanCut.WinApp. It follows the same detailed, numbered "book-style" approach used in `LoadedManagement.md` and then provides a technical deep-dive with code references, DI rationale, and diagrams matching the style used in the management documentation.

Goals:
- Centralize registration of infrastructure, application services and MVP components.
- Make lifetimes and scoping explicit and validated at startup.
- Provide clear rationale for design choices (Transient vs Scoped vs Singleton, generics, factories, logger wiring).

---

# ServiceConfiguration: Narrative guide (book-style, numbered)

Each numbered entry explains the intent, common pitfalls, and a small how-to for implementation.

1) The job of configuration: one authoritative source

- Why: Multiple sources of settings cause confusion. A single `IConfiguration` ensures every part of the app (data, caching, logging, UI) reads the same values, which makes behavior predictable and testable.
- What it does: merges `appsettings.json`, environment variables and any developer overrides into one object passed to all registration helpers.
- How to implement (conceptual):

```csharp
var config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory ?? Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .Build();
services.AddSingleton<IConfiguration>(config);
```

- Common pitfall: reading configuration directly in many places instead of injecting `IConfiguration` — this makes testing harder and leads to duplicated parsing logic.

2) Establish logging early so nothing is lost

- Why: Startup failures are the hardest to diagnose when there is no logging configured. Configuring Serilog early records critical events and makes it safe to call into other subsystems.
- What it does: creates `logs` folder (if needed) and configures console + rolling file sinks, then adds Serilog to the `ILoggingBuilder` so `ILogger<T>` can be injected.
- How to implement (conceptual):

```csharp
Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory ?? ".", "logs"));
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(Path.Combine("logs", "cleanCut-.log"), rollingInterval: RollingInterval.Day)
    .CreateLogger();
services.AddLogging(b => b.AddSerilog());
```

- Note: registering the logger with DI means any presenter or service that requests `ILogger<T>` will get the configured Serilog adapter automatically.

3) Data infrastructure — centralize and respect lifetimes

- Why: Database contexts and repositories are foundational but easy to mis-register. `DbContext` should not be a singleton; it should be scoped to a unit-of-work. In WinForms we don't have an automatic HTTP scope, so scopes must be created explicitly for operations or per-module.
- What it does: registers `DbContext` via `AddDbContext` (scoped by default) and repository interfaces, centralizing connection strings and migration behavior.
- How to implement (conceptual):

```csharp
services.AddDbContext<ApplicationDbContext>((sp, opts) =>
    opts.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
services.AddScoped<ICustomerRepository, CustomerRepository>();
```

- Tip: When running a DB operation from a presenter, create a scope in the `ManagementLoader` or a background task and resolve `ApplicationDbContext` from that scope — do not resolve it as a singleton.

4) Caching infrastructure — design for tiers and fallback

- Why: Caching improves responsiveness. Use an in-memory cache for single-process speed and a distributed cache (Redis) when multiple processes or machines must share state.
- What it does: registers `IMemoryCache`, conditionally configures `IDistributedCache` (Redis) and exposes an `ICacheService` façade that hides composite behavior.
- How to implement (conceptual):

```csharp
services.AddMemoryCache();
if (!string.IsNullOrEmpty(configuration["Redis:Configuration"]))
{
    services.AddStackExchangeRedisCache(opts => opts.Configuration = configuration["Redis:Configuration"]);
    services.AddScoped<ICacheService, RedisCacheService>();
    services.AddScoped<CleanCut.Application.Common.Interfaces.ICacheService, CacheServiceAdapter>();
}
else
{
    services.AddMemoryCache();
    services.AddScoped<ICacheService, MemoryCacheService>();
    services.AddScoped<CleanCut.Application.Common.Interfaces.ICacheService, CacheServiceAdapter>();
}
```

- Note: the application layer consumes `ICacheService` and does not know whether the cache is distributed — that makes tests and local development easier.

> What AddCachingInfrastructure does (concrete):
> - Tests for Redis availability, configures Redis if present.
> - Registers the concrete caching implementation(s) (Redis or memory) and a small adapter so the Application layer consumes `CleanCut.Application.Common.Interfaces.ICacheService` only.
> - Registers helpers such as `CacheKeyBuilder`, `CacheInvalidationService` and any MediatR behaviors that depend on caching when the behavior is registered in the Application host.

### New: Cache invalidation decorator

To demonstrate Open/Closed and support cross-cutting behaviors (logging, metrics) the WinApp registers a decorator around the concrete cache invalidation helper. The concrete `CacheManager` implements the invalidation logic and `LoggingCacheManager` is registered as the `ICacheManager` consumers receive.

DI registration (example):

```csharp
// register concrete implementation
services.AddTransient<CacheManager>();
// register decorator as the ICacheManager that consumers receive
services.AddTransient<ICacheManager>(sp => new LoggingCacheManager(sp.GetRequiredService<CacheManager>(), sp.GetRequiredService<ILogger<LoggingCacheManager>>()));
```

This lets presenters depend on `ICacheManager` while logging and other behaviors are layered on via the decorator — presenters are unchanged.

---

### New: Preferences storage option (file or database)

A small feature was added to allow user preferences (module-level UI preferences) to be stored either on-disk (default) or in the application database. This is intended to demonstrate how to extend behavior via DI without modifying callers (OCP).

- Abstraction: `IUserPreferencesService` (`src/Presentation/CleanCut.WinApp/Services/Management/IUserPreferencesService.cs`)
- File-backed (default): `UserPreferencesService` (`src/Presentation/CleanCut.WinApp/Services/Management/UserPreferencesService.cs`) — registered as a singleton and reads JSON from `%AppData%/CleanCut`.
- DB-backed (optional): `DatabaseUserPreferencesService` (`src/Presentation/CleanCut.WinApp/Services/Management/DatabaseUserPreferencesService.cs`) — reads serialized `UserPreferences` from `UserPreferences` table in the application database.

Switching storage is controlled by a configuration key (default file):

```json
{
  "Preferences": {
    "Store": "file" // change to "db" to use database-backed preferences
  }
}
```

DI selection example (existing `ServiceConfiguration` reads this setting and registers the appropriate implementation):

```csharp
var prefStore = configuration.GetValue<string>("Preferences:Store") ?? "file";
if (prefStore.Equals("db", StringComparison.OrdinalIgnoreCase))
{
    services.AddScoped<IUserPreferencesService, DatabaseUserPreferencesService>();
}
else
{
    services.AddSingleton<IUserPreferencesService, UserPreferencesService>();
}
```

Important notes when enabling DB-backed preferences:
- The database entity `UserPreferenceEntity` (`src/Infrastructure/CleanCut.Infrastructure.Data/Entities/UserPreferenceEntity.cs`) stores a module name and serialized JSON payload. The `CleanCutDbContext` exposes `DbSet<UserPreferences>`.
- You must add and run EF migrations to create the `UserPreferences` table before enabling the DB store in production. Example migration command (from the data project):

```bash
cd src/Infrastructure/CleanCut.Infrastructure.Data
dotnet ef migrations add CreateUserPreferences --project . --startup-project ../../Presentation/CleanCut.WinApp
dotnet ef database update --project . --startup-project ../../Presentation/CleanCut.WinApp
```

- The DB-backed service is registered `Scoped` because it depends on `CleanCutDbContext`.

---

# What AddApplication does (unchanged)

// ... rest of file unchanged ...
