# SOLID Principles — Concrete Examples from CleanCut.WinApp

This document provides real, concrete examples from the `CleanCut.WinApp` project that demonstrate each SOLID principle. For each principle you'll find:
- a short definition,
- one or more concrete examples from the codebase (with file paths),
- why the example satisfies the principle, and
- practical notes or caveats.

This replaces the earlier, high-level list with actionable examples you can inspect in the repo.

---

## Single Responsibility Principle (SRP)

Definition: A class should have one, and only one, reason to change. Keep responsibilities small and focused.

Example 1 — preferences file reader
- File: `src/Presentation/CleanCut.WinApp/Services/Management/UserPreferencesService.cs`
- Responsibility: load user preferences from the filesystem and deserialize JSON.
- Why SRP: this class only knows how to read preferences; it does not own presentation logic or lifecycle management. If storage changes (file → DB), only this class needs to change.

Example 2 — cache invalidation facade
- File: `src/Presentation/CleanCut.WinApp/Services/Caching/CacheManager.cs`
- Responsibility: provide high-level invalidation methods (customers/products/countries/orders).
- Why SRP: CacheManager orchestrates invalidation only — low-level cache storage is delegated to `ICacheService`.

Example 3 — view factory
- File: `src/Presentation/CleanCut.WinApp/Services/Factories/ViewFactory.cs`
- Responsibility: resolve view instances from DI only.

Practical note:
- Designer-generated WinForms files are UI concerns and intentionally include UI wiring; they are allowed to be heavier than SRP for framework reasons. Keep domain/service classes focused.

---

## Open/Closed Principle (OCP)

Definition: Software entities should be open for extension but closed for modification.

Concrete example — preferences storage and cache invalidation decorator
- Abstraction: `src/Presentation/CleanCut.WinApp/Services/Management/IUserPreferencesService.cs`
- File-backed impl: `src/Presentation/CleanCut.WinApp/Services/Management/UserPreferencesService.cs`
- DB-backed impl (new): `src/Presentation/CleanCut.WinApp/Services/Management/DatabaseUserPreferencesService.cs`
- Cache abstraction and decorator:
  - `src/Presentation/CleanCut.WinApp/Services/Caching/ICacheManager.cs`
  - `src/Presentation/CleanCut.WinApp/Services/Caching/CacheManager.cs`
  - `src/Presentation/CleanCut.WinApp/Services/Caching/LoggingCacheManager.cs`
  - DI registration in `src/Presentation/CleanCut.WinApp/Infrastructure/ServiceConfiguration.cs`

Why this demonstrates OCP:
- To add DB-backed preferences we added a new class and registered it in DI based on configuration (`Preferences:Store = "db"`). No callers had to change.
- Added `LoggingCacheManager` decorator to add logging to invalidation without modifying `CacheManager` or presenters.

How to extend safely:
- Prefer small decorators that call an inner implementation and add behavior before/after delegating (logging, metrics, retry).
- Register the concrete implementation and then the decorator in DI so consumers get the decorated instance.

---

## Liskov Substitution Principle (LSP)

Definition: Subtypes must be substitutable for their base types without altering program correctness.

Focused example — `ICacheManager` / `CacheManager` / `LoggingCacheManager`
- Interfaces and implementations:
  - `ICacheManager` (contract)
  - `CacheManager` (orchestrates invalidation via `ICacheService`)
  - `LoggingCacheManager` (decorator) — wraps an `ICacheManager` and delegates.

Why LSP matters here:
- Any `ICacheManager` implementation must perform the documented effect: e.g., `InvalidateCustomersAsync` removes customer-related cache entries. If one impl required callers to call an extra `Flush()` method first, substitution would break callers — a LSP violation.
- The `LoggingCacheManager` preserves the contract by delegating to the inner `ICacheManager` and not changing preconditions or results.

Practical enforcement (implemented):
- Contract tests added at `tests/UnitTests/CleanCut.WinApp.Tests/ContractTests/CacheManagerContractTests.cs` verify that both `CacheManager` and `LoggingCacheManager` remove seeded keys as expected.

---

## Interface Segregation Principle (ISP)

Definition: Clients should not be forced to depend on interfaces they do not use. Prefer many small, focused interfaces over a fat one.

Concrete examples from WinApp:
- `IUserPreferencesService` — single method `LoadPreferencesAsync`.
- `ICacheManager` — exposes only invalidation methods presenters need.
- View interfaces (e.g., `ICustomerListView`) and factory `IViewFactory<TView>` allow presenters to depend only on required members.

Why ISP helps here:
- Presenters can be unit-tested by mocking the small interfaces they actually consume.
- Implementers of these small interfaces are simpler to write and reason about.

---

## Dependency Inversion Principle (DIP)

Definition: High-level modules should not depend on low-level modules; both should depend on abstractions.

Concrete example — loader and preferences
- High-level: `ManagementLoader` depends on `IUserPreferencesService`.
- Low-level: `UserPreferencesService` (file) and `DatabaseUserPreferencesService` (DB) implement the abstraction.

Why DIP is satisfied:
- `ManagementLoader` depends on the abstraction; to change storage you register a different implementation in DI.

Other DIP uses in WinApp:
- Presenters depend on `IMediator`, `ICacheService`, `ICacheManager`, and `IViewFactory<TView>` abstractions.

---

## Updates to documentation and migration notes

Because a DB-backed preferences store was added, update these docs if you will switch to DB in production:
- Add EF Core migration to create the `UserPreferences` table. See `src/Infrastructure/CleanCut.Infrastructure.Data/Entities/UserPreferenceEntity.cs`.
- Update deployment docs (README) to include running migrations and ensuring the connection string is configured.

Example migration command:
```bash
cd src/Infrastructure/CleanCut.Infrastructure.Data
dotnet ef migrations add CreateUserPreferences --project . --startup-project ../../Presentation/CleanCut.WinApp
dotnet ef database update --project . --startup-project ../../Presentation/CleanCut.WinApp
```

---

## Appendix: concrete code references (quick list)
- `src/Presentation/CleanCut.WinApp/Services/Management/UserPreferencesService.cs` — SRP, DIP (file-backed)
- `src/Presentation/CleanCut.WinApp/Services/Management/DatabaseUserPreferencesService.cs` — OCP demonstration (DB-backed)
- `src/Infrastructure/CleanCut.Infrastructure.Data/Entities/UserPreferenceEntity.cs` — DB entity
- `src/Infrastructure/CleanCut.Infrastructure.Data/Context/CleanCutDbContext.cs` — DbSet<UserPreferences>
- `src/Presentation/CleanCut.WinApp/Services/Caching/ICacheManager.cs` — ISP, DIP
- `src/Presentation/CleanCut.WinApp/Services/Caching/CacheManager.cs` — SRP, DIP
- `src/Presentation/CleanCut.WinApp/Services/Caching/LoggingCacheManager.cs` — OCP (decorator)
- `src/Presentation/CleanCut.WinApp/Services/Factories/ViewFactory.cs` — SRP, DIP, ISP
- `src/Presentation/CleanCut.WinApp/Presenters/CustomerListPresenter.cs` — DIP, ISP, SRP (presenter-specific concerns)
- `tests/UnitTests/CleanCut.WinApp.Tests/ContractTests/CacheManagerContractTests.cs` — LSP contract tests

---

If you'd like, I can:
1. Add the EF Core migration file for `UserPreferences` and update the README migration instructions.
2. Add persistence methods for saving preferences to DB (update `IUserPreferencesService` to include a `SavePreferencesAsync` method and implement it in both file and DB services).
3. Expand contract tests to include products/countries/orders invalidation and cancellation token handling.

Which should I do next?
